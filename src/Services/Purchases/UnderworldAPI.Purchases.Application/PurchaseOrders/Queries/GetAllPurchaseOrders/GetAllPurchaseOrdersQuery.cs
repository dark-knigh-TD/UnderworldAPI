using System;
using MediatR;
using UnderworldAPI.Purchases.Application.DTOs;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Application.PurchaseOrders.Queries.GetAllPurchaseOrders;

public sealed record GetAllPurchaseOrdersQuery()
    : IRequest<Result<IEnumerable<PurchaseOrderDto>>>;

