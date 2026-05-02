using System;
using System.Security.Principal;
using UnderworldAPI.Sales.Domain.Events;
using UnderworldAPI.Sales.Domain.ValueObjects;
using UnderworldAPI.Shared.Domain.Primitives;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Domain.Aggregates.Order;

public sealed  class Order:AggregateRoot
{
    private readonly List<OrderItem> _items = [];
    
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public Money TotalAmount => _items.Count == 0
        ? Money.Zero()
        : _items.Skip(1).Aggregate(
            _items[0].Total,
            (acc, item) => acc.Add(item.Total));

    // Constructors
    private Order() { } // EF Core
    public Order(Guid id, CustomerId customerId):base(id)
    {
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;

    }
    

    public static Result<Order> Create(Guid customerId)
    {
        var customerIdResult = CustomerId.Create(customerId);
        if (customerIdResult.IsFailure)
            return Result.Failure<Order>(customerIdResult.Error);

        var order = new Order(Guid.NewGuid(), customerIdResult.Value);

        order.RaiseDomainEvent(new OrderCreatedEvent(
            order.Id,
            customerIdResult.Value.Value,
            order.CreatedAt));

        return Result.Success(order);
    }

    public Result AddItem(Guid productId, string productName, Money unitPrice, int quantity)
    {   
        if (Status == OrderStatus.Cancelled)
            return Result.Failure(
                Error.Conflict(nameof(Order), "Cannot add items to a cancelled order."));

        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            return Result.Failure(
                Error.Conflict(nameof(Order), $"Cannot modify an order in status {Status}."));

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            var itemResult = OrderItem.Create(Id, productId, productName, unitPrice, quantity);
            if (itemResult.IsFailure)
                return Result.Failure(itemResult.Error);

            _items.Add(itemResult.Value);
        }

        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new OrderUpdatedEvent(
            Id,
            CustomerId.Value,
            UpdatedAt.Value));

        return Result.Success();
    }

    public Result Confirm()
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(
                Error.Conflict(nameof(Order), $"Only Pending orders can be confirmed. Current status: {Status}."));

        if (!_items.Any())
            return Result.Failure(
                Error.Conflict(nameof(Order), "Cannot confirm an order with no items."));


        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new OrderUpdatedEvent(
            Id,
            CustomerId.Value,
            UpdatedAt.Value));

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            return Result.Failure(
                Error.Conflict(nameof(Order), "Order is already cancelled."));

        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            return Result.Failure(
                Error.Conflict(nameof(Order), $"Cannot cancel an order in status {Status}."));

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new OrderCancelledEvent(
            Id,
            CustomerId.Value,
            UpdatedAt.Value));

        return Result.Success();
    }

    public Result MarkAsShipped()
    {
        if (Status != OrderStatus.Confirmed)
            return Result.Failure(
                Error.Conflict(nameof(Order), $"Only Confirmed orders can be marked as shipped. Current status: {Status}."));

        Status = OrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new OrderUpdatedEvent(
            Id,
            CustomerId.Value,
            UpdatedAt.Value));

        return Result.Success();
    }
}
