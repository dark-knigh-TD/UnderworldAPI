using System;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using UnderworldAPI.Sales.Application.Abstractions;
using UnderworldAPI.Sales.Domain.Events;
using UnderworldAPI.Shared.Domain.Primitives;

namespace UnderworldAPI.Sales.Infrastructure.Messaging;

internal sealed class AzureServiceBusPublisher(
    ServiceBusClient serviceBusClient,
    ILogger<AzureServiceBusPublisher> logger
) : IEventPublisher
{

     private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
    {
        var topicName = GetTopicName<TEvent>();
        await using var sender = serviceBusClient.CreateSender(topicName);

        // Construir el mensaje según el tipo de evento
        var messageBody = BuildMessageBody(domainEvent);

        var message = new ServiceBusMessage(messageBody)
        {
            MessageId = domainEvent.Id.ToString(),
            Subject = typeof(TEvent).Name,
            ContentType = "application/json"
        };

        try
        {
            await sender.SendMessageAsync(message, cancellationToken);

            logger.LogInformation(
                "Domain event {EventType} with Id {EventId} published to topic {TopicName}",
                typeof(TEvent).Name,
                domainEvent.Id,
                topicName);
        }
        catch (ServiceBusException ex)
        {
            logger.LogError(ex,
                "Failed to publish domain event {EventType} with Id {EventId} to topic {TopicName}",
                typeof(TEvent).Name,
                domainEvent.Id,
                topicName);

            throw; // Re-lanzar — el handler de la request capturará el error
        }
    }

    private static string BuildMessageBody<TEvent>(TEvent domainEvent)
        where TEvent : IDomainEvent
    {
        // Para OrderCreatedEvent publicamos el IntegrationEvent con los items
        if (domainEvent is OrderCreatedEvent orderCreatedEvent)
        {
            // El Order no viene en el evento — necesitamos el contexto
            // Por eso usamos un approach diferente — ver nota abajo
            return JsonSerializer.Serialize(domainEvent, JsonOptions);
        }

        return JsonSerializer.Serialize(domainEvent, JsonOptions);
    }
    private static string GetTopicName<TEvent>() =>
        typeof(TEvent).Name
            .Replace("Event", string.Empty)
            .Aggregate(
                string.Empty,
                (acc, c) => acc + (char.IsUpper(c) && acc.Length > 0 ? "-" : string.Empty) + char.ToLower(c));
}
