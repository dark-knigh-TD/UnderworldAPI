using System;
using AutoMapper;
using MediatR;
using UnderworldAPI.Sales.Application.Abstractions;
using UnderworldAPI.Sales.Domain.Aggregates.Order;
using UnderworldAPI.Sales.Domain.Ports.Repositories;
using UnderworldAPI.Sales.Domain.ValueObjects;
using UnderworldAPI.Shared.Domain.IntegrationEvents;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Application.Orders.Commands.CreateOrder;

internal sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IIntegrationEventPublisher integrationEventPublisher
) : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderResult = Order.Create(request.CustomerId);
        if (orderResult.IsFailure)
            return Result.Failure<Guid>(orderResult.Error);

        var order = orderResult.Value;

        foreach (var item in request.Items)
        {
            var moneyResult = Money.Create(item.UnitPrice, item.Currency);
            if (moneyResult.IsFailure)
                return Result.Failure<Guid>(moneyResult.Error);

            var addItemResult = order.AddItem(
                item.ProductId,
                item.ProductName,
                moneyResult.Value,
                item.Quantity);

            if (addItemResult.IsFailure)
                return Result.Failure<Guid>(addItemResult.Error);
        }

        await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Publicar Integration Event con el payload completo
        // Esto dispara el reabastecimiento automático en Purchases
        var integrationEvent = new OrderCreatedIntegrationEvent(
            OrderId: order.Id,
            CustomerId: order.CustomerId.Value,
            OccurredAt: order.CreatedAt,
            Items: request.Items.Select(i => new OrderItemIntegrationEvent(
                ProductId: i.ProductId,
                ProductName: i.ProductName,
                UnitPrice: i.UnitPrice,
                Currency: i.Currency,
                Quantity: i.Quantity
            )).ToList()
        );

        await integrationEventPublisher.PublishOrderCreatedAsync(
            integrationEvent, cancellationToken);


        return Result.Success(order.Id);
    }
}
