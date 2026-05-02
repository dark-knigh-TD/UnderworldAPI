using System;
using MediatR;
using UnderworldAPI.Sales.Application.Abstractions;
using UnderworldAPI.Sales.Domain.Aggregates.Order;
using UnderworldAPI.Sales.Domain.Ports.Repositories;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Application.Orders.Commands.UpdateOrder;

internal sealed class UpdateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork
    ):IRequestHandler<UpdateOrderCommand, Result>
{
    public async Task<Result> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
         return Result.Failure(Error.NotFound(nameof(Order), request.OrderId));


        var result = request.Action switch
            {
                "Confirm" => order.Confirm(),
                "Ship"    => order.MarkAsShipped(),
                "Cancel"  => order.Cancel(),
                _         => Result.Failure(Error.Validation(nameof(request.Action), $"Unknown action: {request.Action}."))
            };

        // switch (request.Action)
        // {
        //     case "Confirm":
        //         order.Confirm();
        //         break;
        //     case "Ship":
        //         order.Ship();
        //         break;
        //     case "Cancel":
        //         order.Cancel();
        //         break;
        //     default:
        //         return Result.Failure("Invalid action.");
        // }
        if (result.IsFailure)
            return result;

        await orderRepository.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    
    }
}
