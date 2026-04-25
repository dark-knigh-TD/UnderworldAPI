using System;
using MediatR;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderCommand(
   Guid SupplierId,
    string? Notes,
    List<CreatePurchaseOrderItemRequest> Items
): IRequest<Result<Guid>>;

public sealed record CreatePurchaseOrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity
);
