using System;
using MediatR;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand
(
    Guid CustomerId,
    IReadOnlyList<CreateOrderItemRequest> Items
):IRequest<Result<Guid>>;

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity
);