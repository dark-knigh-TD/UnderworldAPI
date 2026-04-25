using System;
using MediatR;
using UnderworldAPI.Purchases.Application.Abstractions;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;
using UnderworldAPI.Purchases.Domain.Ports.Repositories;
using UnderworldAPI.Purchases.Domain.ValueObjects;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.CreatePurchaseOrder;

internal sealed class CreatePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreatePurchaseOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrderResult = PurchaseOrder.Create(request.SupplierId, request.Notes);
        if (purchaseOrderResult.IsFailure)
            return Result.Failure<Guid>(purchaseOrderResult.Error);

        var purchaseOrder = purchaseOrderResult.Value;

        foreach (var item in request.Items)
        {
            var moneyResult = Money.Create(item.UnitPrice, item.Currency);
            if (moneyResult.IsFailure)
                return Result.Failure<Guid>(moneyResult.Error);

            var addItemResult = purchaseOrder.AddItem(
                item.ProductId,
                item.ProductName,
                moneyResult.Value,
                item.Quantity);

            if (addItemResult.IsFailure)
                return Result.Failure<Guid>(addItemResult.Error);
        }

        await purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(purchaseOrder.Id);
    }
}
