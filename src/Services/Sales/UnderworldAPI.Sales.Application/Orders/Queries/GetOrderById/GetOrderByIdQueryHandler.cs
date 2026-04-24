using System;
using AutoMapper;
using MediatR;
using UnderworldAPI.Sales.Application.Abstractions;
using UnderworldAPI.Sales.Application.DTOs;
using UnderworldAPI.Sales.Domain.Aggregates.Order;
using UnderworldAPI.Sales.Domain.Ports.Repositories;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Application.Orders.Queries.GetOrderById;

internal sealed class GetOrderByIdQueryHandler(
    IOrderRepository orderRepository,
    IMapper mapper) : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
            return Result.Failure<OrderDto>(Error.NotFound(nameof(Order), request.OrderId));

        return Result.Success(mapper.Map<OrderDto>(order));
    }
}
