using System.Text.Json;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using UnderworldAPI.Shared.Domain.IntegrationEvents;

namespace UnderworldAPI.Purchases.Infrastructure.Messaging;

/// <summary>
/// Background Service que escucha el topic sales-events de Azure Service Bus.
/// Cuando recibe un OrderCreatedIntegrationEvent, automáticamente crea
/// una PurchaseOrder al proveedor para reabastecer el inventario vendido.
/// </summary>
public sealed class SalesEventsConsumer(
    ServiceBusClient serviceBusClient,
    IServiceScopeFactory scopeFactory,
    ILogger<SalesEventsConsumer> logger
) : BackgroundService
{
    private const string TopicName = "sales-events";
    private const string SubscriptionName = "purchases-restock-subscription";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "SalesEventsConsumer started. Listening to topic {Topic} subscription {Subscription}",
            TopicName, SubscriptionName);

        var processorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        };

        await using var processor = serviceBusClient.CreateProcessor(
            TopicName,
            SubscriptionName,
            processorOptions);

        processor.ProcessMessageAsync += HandleMessageAsync;
        processor.ProcessErrorAsync += HandleErrorAsync;

        await processor.StartProcessingAsync(stoppingToken);

        // Mantener el servicio corriendo hasta que se cancele
        await Task.Delay(Timeout.Infinite, stoppingToken);

        await processor.StopProcessingAsync(stoppingToken);
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        var subject = args.Message.Subject;
        var body = args.Message.Body.ToString();

        logger.LogInformation(
            "Received message from sales-events. Subject: {Subject}", subject);

        try
        {
            if (subject == nameof(OrderCreatedIntegrationEvent))
            {
                await HandleOrderCreatedAsync(body, args.CancellationToken);
            }

            // Marcar el mensaje como procesado exitosamente
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing message with Subject {Subject}", subject);

            // Abandonar el mensaje para que Service Bus haga retry
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private async Task HandleOrderCreatedAsync(
        string messageBody,
        CancellationToken cancellationToken)
    {
        var integrationEvent = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(
            messageBody, JsonOptions);

        if (integrationEvent is null)
        {
            logger.LogWarning("Could not deserialize OrderCreatedIntegrationEvent");
            return;
        }

        logger.LogInformation(
            "Processing OrderCreatedIntegrationEvent for OrderId {OrderId} " +
            "with {ItemCount} items. Creating restock PurchaseOrder.",
            integrationEvent.OrderId,
            integrationEvent.Items.Count);

        // Usar IServiceScopeFactory porque BackgroundService es Singleton
        // pero los handlers son Scoped
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        // Crear la PurchaseOrder automáticamente con los mismos
        // productos y cantidades de la venta
        var command = new CreatePurchaseOrderCommand(
            SupplierId: Guid.NewGuid(), // En producción vendría de un catálogo de proveedores
            Notes: $"Auto-restock for OrderId: {integrationEvent.OrderId}",
            Items: integrationEvent.Items.Select(i =>
                new CreatePurchaseOrderItemRequest(
                    ProductId: i.ProductId,
                    ProductName: i.ProductName,
                    UnitPrice: i.UnitPrice,
                    Currency: i.Currency,
                    Quantity: i.Quantity
                )).ToList()
        );

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation(
                "Restock PurchaseOrder {PurchaseOrderId} created successfully " +
                "for OrderId {OrderId}",
                result.Value,
                integrationEvent.OrderId);
        }
        else
        {
            logger.LogError(
                "Failed to create restock PurchaseOrder for OrderId {OrderId}. " +
                "Error: {Error}",
                integrationEvent.OrderId,
                result.Error);
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception,
            "Service Bus error on {EntityPath}. Source: {ErrorSource}",
            args.EntityPath,
            args.ErrorSource);

        return Task.CompletedTask;
    }
}