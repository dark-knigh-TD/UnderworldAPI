using System;
using MediatR;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;

public sealed record  UpdatePurchaseOrderCommand(
     Guid PurchaseOrderId,
    string Action  // "Send" | "Confirm" | "Cancel"
):IRequest<Result>;
