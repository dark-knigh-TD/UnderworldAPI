using System;
using FluentValidation;

namespace UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.DeletePurchaseOrder;

public sealed class DeletePurchaseOrderCommandValidator:AbstractValidator<DeletePurchaseOrderCommand>
{
    public DeletePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase order id is required.")
            .NotEqual(Guid.Empty).WithMessage("Purchase order id cannot be empty.");
    }
}
