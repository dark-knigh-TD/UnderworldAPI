using System;

namespace UnderworldAPI.Purchases.Application.DTOs;

public sealed record PurchaseOrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    int? ReceivedQuantity,
    decimal Total
);
