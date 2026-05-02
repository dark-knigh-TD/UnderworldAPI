using System;
using AutoMapper;
using MediatR;
using UnderworldAPI.Purchases.Application.DTOs;
using UnderworldAPI.Purchases.Domain.Ports.Repositories;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Application.PurchaseOrders.Queries.GetAllPurchaseOrders;

internal sealed class GetAllPurchaseOrdersQueryHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IMapper mapper
): IRequestHandler<GetAllPurchaseOrdersQuery, Result<IEnumerable<PurchaseOrderDto>>>
{
    public async Task<Result<IEnumerable<PurchaseOrderDto>>> Handle(GetAllPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var purchaseOrders = await purchaseOrderRepository.GetAllAsync(cancellationToken);
        var purchaseOrderDtos = mapper.Map<IEnumerable<PurchaseOrderDto>>(purchaseOrders);
        return Result.Success(purchaseOrderDtos);
    }
}

