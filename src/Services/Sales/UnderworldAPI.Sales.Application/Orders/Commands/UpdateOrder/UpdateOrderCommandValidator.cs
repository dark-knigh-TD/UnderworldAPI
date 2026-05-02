using System;
using FluentValidation;

namespace UnderworldAPI.Sales.Application.Orders.Commands.UpdateOrder;

public sealed class UpdateOrderCommandValidator:AbstractValidator<UpdateOrderCommand>
{
    private static readonly string[] ValidActions = ["Confirm", "Ship", "Cancel"];
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.")
            .Must(id => id != Guid.Empty).WithMessage("OrderId must be a valid GUID.");

       RuleFor(x => x.Action)
            .NotEmpty()
            .Must(action => ValidActions.Contains(action))
            .WithMessage($"Action must be one of: {string.Join(", ", ValidActions)}.");
    }

}
