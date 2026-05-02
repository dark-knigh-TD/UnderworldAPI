using System;

namespace UnderworldAPI.Purchases.Application.DTOs;

public sealed record PurchaseOrderDto{
   public Guid Id { get; init; }
    public Guid SupplierId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IReadOnlyList<PurchaseOrderItemDto> Items { get; init; } = [];
}
