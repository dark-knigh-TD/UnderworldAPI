using System;
using FluentValidation;

namespace UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderCommandValidator:AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty()
            .WithMessage("SupplierId is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes is not null)
            .WithMessage("Notes must not exceed 500 characters.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Purchase order must have at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("ProductId is required.");

            item.RuleFor(x => x.ProductName)
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("Product name is required and must not exceed 200 characters.");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThan(0)
                .WithMessage("Unit price must be greater than zero.");

            item.RuleFor(x => x.Currency)
                .NotEmpty()
                .Length(3)
                .WithMessage("Currency must be a 3-letter ISO code.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        });
    }
}
