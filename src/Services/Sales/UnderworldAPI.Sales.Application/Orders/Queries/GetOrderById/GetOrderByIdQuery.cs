using System;
using MediatR;
using UnderworldAPI.Sales.Application.DTOs;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderDto>>;
