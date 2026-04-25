using System;
using MediatR;
using UnderworldAPI.Purchases.Application.DTOs;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Application.PurchaseOrders.Queries.GetPurchaseOrderById;

public sealed record GetPurchaseOrderByIdQuery(
    Guid PurchaseOrderId
):IRequest<Result<PurchaseOrderDto>>;

