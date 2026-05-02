using System;
using AutoMapper;
using MediatR;
using UnderworldAPI.Purchases.Application.DTOs;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;
using UnderworldAPI.Purchases.Domain.Ports.Repositories;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Application.PurchaseOrders.Queries.GetPurchaseOrderById;

internal sealed class GetPurchaseOrderByIdQueryHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IMapper mapper
):IRequestHandler<GetPurchaseOrderByIdQuery, Result<PurchaseOrderDto>>
{
    public async Task<Result<PurchaseOrderDto>> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId, cancellationToken);

        if (purchaseOrder is null)
            return Result.Failure<PurchaseOrderDto>(Error.NotFound(nameof(PurchaseOrder), request.PurchaseOrderId));

        var purchaseOrderDto = mapper.Map<PurchaseOrderDto>(purchaseOrder);
        return Result.Success(purchaseOrderDto);
    }
}
