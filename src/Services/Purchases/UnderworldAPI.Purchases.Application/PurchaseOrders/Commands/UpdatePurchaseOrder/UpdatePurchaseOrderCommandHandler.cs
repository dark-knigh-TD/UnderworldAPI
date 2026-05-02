using System;
using MediatR;
using UnderworldAPI.Purchases.Application.Abstractions;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;
using UnderworldAPI.Purchases.Domain.Ports.Repositories;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;

internal sealed class UpdatePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdatePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(UpdatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId, cancellationToken);

        if (purchaseOrder is null)
        return Result.Failure(
                Error.NotFound(nameof(PurchaseOrder), request.PurchaseOrderId));

        var result = request.Action switch
        {
            "Send"    => purchaseOrder.Send(),
            "Confirm" => purchaseOrder.Confirm(),
            "Cancel"  => purchaseOrder.Cancel(),
            _         => Result.Failure(Error.Validation(
                             nameof(request.Action),
                             $"Unknown action: {request.Action}."))
        };

        if (result.IsFailure)
        return result;

        await purchaseOrderRepository.UpdateAsync(purchaseOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);   

        return Result.Success();  
    }
}
