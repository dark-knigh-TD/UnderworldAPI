using System;
using FluentValidation;

namespace UnderworldAPI.Sales.Application.Orders.Commands.DeleteOrder;

public sealed class DeleteOrderCommandValidator:AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required.");
    }
}
