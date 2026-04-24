using System;
using AutoMapper;
using MediatR;
using UnderworldAPI.Sales.Application.DTOs;
using UnderworldAPI.Sales.Domain.Ports.Repositories;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Application.Orders.Queries.GetAllOrders;

internal sealed class GetAllOrdersQueryHandler(
    IOrderRepository orderRepository,
    IMapper mapper) : IRequestHandler<GetAllOrdersQuery, Result<IEnumerable<OrderDto>>>
{
    public async Task<Result<IEnumerable<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<OrderDto>>(orders));
    }

}