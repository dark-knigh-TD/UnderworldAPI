using System;
using MediatR;
using UnderworldAPI.Purchases.Application.Abstractions;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;
using UnderworldAPI.Purchases.Domain.Ports.Repositories;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.DeletePurchaseOrder;

internal sealed class DeletePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IUnitOfWork unitOfWork
):IRequestHandler<DeletePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(DeletePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId, cancellationToken);

        if (purchaseOrder is null)
            return Result.Failure(Error.NotFound(nameof(PurchaseOrder), request.PurchaseOrderId));

        if (purchaseOrder.Status != PurchaseOrderStatus.Cancelled)
            return Result.Failure(
                Error.Conflict(nameof(PurchaseOrder),
                    "Only cancelled purchase orders can be deleted."));

        await purchaseOrderRepository.DeleteAsync(purchaseOrder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

