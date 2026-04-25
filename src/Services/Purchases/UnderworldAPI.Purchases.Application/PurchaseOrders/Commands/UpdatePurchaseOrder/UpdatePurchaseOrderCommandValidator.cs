using System;
using FluentValidation;

namespace UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;

public sealed class UpdatePurchaseOrderCommandValidator:AbstractValidator<UpdatePurchaseOrderCommand>
{
     private static readonly string[] ValidActions = ["Send", "Confirm", "Cancel"];

    public UpdatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty()
            .WithMessage("PurchaseOrderId is required.");

        RuleFor(x => x.Action)
            .NotEmpty()
            .Must(action => ValidActions.Contains(action))
            .WithMessage($"Action must be one of: {string.Join(", ", ValidActions)}.");
    }
}
