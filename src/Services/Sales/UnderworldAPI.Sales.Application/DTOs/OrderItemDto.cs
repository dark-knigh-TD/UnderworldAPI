using System;

namespace UnderworldAPI.Sales.Application.DTOs;

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal Total
);
