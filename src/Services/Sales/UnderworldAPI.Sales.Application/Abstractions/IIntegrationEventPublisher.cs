using UnderworldAPI.Shared.Domain.IntegrationEvents;

namespace UnderworldAPI.Sales.Application.Abstractions;

/// <summary>
/// Puerto para publicar Integration Events al Service Bus.
/// A diferencia de IEventPublisher que publica Domain Events,
/// este publica eventos con el payload completo para otros microservicios.
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishOrderCreatedAsync(
        OrderCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}