using System;
using MediatR;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Application.Orders.Commands.DeleteOrder;

public sealed record DeleteOrderCommand(
    Guid OrderId
    ):IRequest<Result>;

