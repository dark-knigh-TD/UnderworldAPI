using System;
using MediatR;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Application.Orders.Commands.UpdateOrder;

public sealed record UpdateOrderCommand(
    Guid OrderId,
    string Action  
):IRequest<Result>;
