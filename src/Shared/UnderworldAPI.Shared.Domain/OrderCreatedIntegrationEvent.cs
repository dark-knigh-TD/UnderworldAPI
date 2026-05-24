namespace UnderworldAPI.Shared.Domain.IntegrationEvents;

/// <summary>
/// Integration Event — diferente a Domain Event.
/// Domain Event: interno al microservicio.
/// Integration Event: cruza fronteras entre microservicios via Service Bus.
/// </summary>
public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    DateTime OccurredAt,
    List<OrderItemIntegrationEvent> Items
);

public sealed record OrderItemIntegrationEvent(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity
);