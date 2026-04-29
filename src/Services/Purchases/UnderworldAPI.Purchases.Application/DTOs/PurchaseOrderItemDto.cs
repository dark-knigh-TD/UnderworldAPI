using System;

namespace UnderworldAPI.Purchases.Application.DTOs;

public sealed record PurchaseOrderItemDto{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public int? ReceivedQuantity { get; init; }
    public decimal Total { get; init; }
}
