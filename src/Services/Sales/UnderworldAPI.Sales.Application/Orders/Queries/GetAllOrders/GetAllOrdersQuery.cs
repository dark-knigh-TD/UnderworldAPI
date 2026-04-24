using System;
using MediatR;
using UnderworldAPI.Sales.Application.DTOs;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Application.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery:IRequest<Result<IEnumerable<OrderDto>>>;

