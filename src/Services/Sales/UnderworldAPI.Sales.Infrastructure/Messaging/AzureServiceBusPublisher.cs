using System;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using UnderworldAPI.Sales.Application.Abstractions;
using UnderworldAPI.Shared.Domain.Primitives;

namespace UnderworldAPI.Sales.Infrastructure.Messaging;

internal sealed class AzureServiceBusPublisher(
    ServiceBusClient serviceBusClient,
    ILogger<AzureServiceBusPublisher> logger
) : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
    {
        var topicName = GetTopicName<TEvent>();

        await using var sender = serviceBusClient.CreateSender(topicName);

        var messageBody = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

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

    private static string GetTopicName<TEvent>() =>
        typeof(TEvent).Name
            .Replace("Event", string.Empty)
            .Aggregate(
                string.Empty,
                (acc, c) => acc + (char.IsUpper(c) && acc.Length > 0 ? "-" : string.Empty) + char.ToLower(c));
}
