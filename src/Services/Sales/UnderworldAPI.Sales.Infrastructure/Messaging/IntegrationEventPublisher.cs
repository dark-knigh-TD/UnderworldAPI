using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using UnderworldAPI.Sales.Application.Abstractions;
using UnderworldAPI.Shared.Domain.IntegrationEvents;

namespace UnderworldAPI.Sales.Infrastructure.Messaging;

/// <summary>
/// Publica Integration Events al Service Bus con el payload completo.
/// Incluye todos los datos que otros microservicios necesitan
/// sin tener que hacer llamadas adicionales.
/// </summary>
internal sealed class IntegrationEventPublisher(
    ServiceBusClient serviceBusClient,
    ILogger<IntegrationEventPublisher> logger
) : IIntegrationEventPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task PublishOrderCreatedAsync(
        OrderCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        const string topicName = "sales-events";
        await using var sender = serviceBusClient.CreateSender(topicName);

        var messageBody = JsonSerializer.Serialize(integrationEvent, JsonOptions);

        var message = new ServiceBusMessage(messageBody)
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = nameof(OrderCreatedIntegrationEvent),
            ContentType = "application/json"
        };

        try
        {
            await sender.SendMessageAsync(message, cancellationToken);
            logger.LogInformation(
                "OrderCreatedIntegrationEvent published for OrderId {OrderId}",
                integrationEvent.OrderId);
        }
        catch (ServiceBusException ex)
        {
            logger.LogError(ex,
                "Failed to publish OrderCreatedIntegrationEvent for OrderId {OrderId}",
                integrationEvent.OrderId);
            throw;
        }
    }
}