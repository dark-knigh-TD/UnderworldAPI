using System;

namespace UnderworldAPI.Sales.Application.DTOs;

public sealed record OrderDto{
   public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IReadOnlyList<OrderItemDto> Items { get; init; } = [];
}

