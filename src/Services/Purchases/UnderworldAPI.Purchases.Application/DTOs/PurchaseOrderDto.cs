using System;

namespace UnderworldAPI.Purchases.Application.DTOs;

public sealed record PurchaseOrderDto(
    Guid Id,
    Guid SupplierId,
    string Status,
    decimal TotalAmount,
    string Currency,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<PurchaseOrderItemDto> Items
);
