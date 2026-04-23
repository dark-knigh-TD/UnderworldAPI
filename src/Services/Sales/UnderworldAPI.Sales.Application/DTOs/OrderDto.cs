using System;

namespace UnderworldAPI.Sales.Application.DTOs;

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<OrderItemDto> Items
);

