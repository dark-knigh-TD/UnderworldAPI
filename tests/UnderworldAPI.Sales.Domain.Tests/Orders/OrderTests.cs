using FluentAssertions;
using UnderworldAPI.Sales.Domain.Aggregates.Order;
using UnderworldAPI.Sales.Domain.Events;
using UnderworldAPI.Sales.Domain.ValueObjects;

namespace UnderworldAPI.Sales.Domain.Tests.Orders;

public class OrderTests
{
    private static readonly Guid ValidCustomerId = Guid.NewGuid();
    private static readonly Guid ValidProductId = Guid.NewGuid();
    private static readonly Money ValidMoney = Money.Create(100m, "MXN").Value;

    // ── Create ────────────────────────────────────────────────
    [Fact]
    public void Create_WithValidCustomerId_ShouldReturnSuccess()
    {
        var result = Order.Create(ValidCustomerId);

        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerId.Value.Should().Be(ValidCustomerId);
        result.Value.Status.Should().Be(OrderStatus.Pending);
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldReturnFailure()
    {
        var result = Order.Create(Guid.Empty);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldRaiseOrderCreatedEvent()
    {
        var result = Order.Create(ValidCustomerId);

        result.Value.GetDomainEvents()
            .Should().ContainSingle(e => e is OrderCreatedEvent);
    }

    // ── AddItem ───────────────────────────────────────────────
    [Fact]
    public void AddItem_WithValidData_ShouldAddItemToOrder()
    {
        var order = Order.Create(ValidCustomerId).Value;

        var result = order.AddItem(ValidProductId, "Product A", ValidMoney, 2);

        result.IsSuccess.Should().BeTrue();
        order.Items.Should().HaveCount(1);
        order.Items[0].Quantity.Should().Be(2);
    }

    [Fact]
    public void AddItem_WithSameProduct_ShouldIncreaseQuantity()
    {
        var order = Order.Create(ValidCustomerId).Value;
        order.AddItem(ValidProductId, "Product A", ValidMoney, 2);

        order.AddItem(ValidProductId, "Product A", ValidMoney, 3);

        order.Items.Should().HaveCount(1);
        order.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_WithDifferentProducts_ShouldAddSeparateItems()
    {
        var order = Order.Create(ValidCustomerId).Value;

        order.AddItem(ValidProductId, "Product A", ValidMoney, 1);
        order.AddItem(Guid.NewGuid(), "Product B", ValidMoney, 1);

        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void AddItem_ToCancelledOrder_ShouldReturnFailure()
    {
        var order = Order.Create(ValidCustomerId).Value;
        order.AddItem(ValidProductId, "Product A", ValidMoney, 1);
        order.Confirm();
        order.Cancel();

        var result = order.AddItem(Guid.NewGuid(), "Product B", ValidMoney, 1);

        result.IsFailure.Should().BeTrue();
    }

    // ── TotalAmount ───────────────────────────────────────────
    [Fact]
    public void TotalAmount_WithMultipleItems_ShouldReturnCorrectTotal()
    {
        var order = Order.Create(ValidCustomerId).Value;
        var money50 = Money.Create(50m, "MXN").Value;
        var money100 = Money.Create(100m, "MXN").Value;

        order.AddItem(Guid.NewGuid(), "Product A", money50, 2);
        order.AddItem(Guid.NewGuid(), "Product B", money100, 1);

        order.TotalAmount.Amount.Should().Be(200m);
    }

    // ── Confirm ───────────────────────────────────────────────
    [Fact]
    public void Confirm_PendingOrderWithItems_ShouldSucceed()
    {
        var order = Order.Create(ValidCustomerId).Value;
        order.AddItem(ValidProductId, "Product A", ValidMoney, 1);

        var result = order.Confirm();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_OrderWithNoItems_ShouldReturnFailure()
    {
        var order = Order.Create(ValidCustomerId).Value;

        var result = order.Confirm();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Confirm_AlreadyConfirmedOrder_ShouldReturnFailure()
    {
        var order = Order.Create(ValidCustomerId).Value;
        order.AddItem(ValidProductId, "Product A", ValidMoney, 1);
        order.Confirm();

        var result = order.Confirm();

        result.IsFailure.Should().BeTrue();
    }

    // ── Cancel ────────────────────────────────────────────────
    [Fact]
    public void Cancel_PendingOrder_ShouldSucceed()
    {
        var order = Order.Create(ValidCustomerId).Value;

        var result = order.Cancel();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_AlreadyCancelledOrder_ShouldReturnFailure()
    {
        var order = Order.Create(ValidCustomerId).Value;
        order.Cancel();

        var result = order.Cancel();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Cancel_ShouldRaiseOrderCancelledEvent()
    {
        var order = Order.Create(ValidCustomerId).Value;
        order.ClearDomainEvents();

        order.Cancel();

        order.GetDomainEvents()
        .OfType<OrderCancelledEvent>()
        .Should().HaveCount(1);
    }

    // ── MarkAsShipped ─────────────────────────────────────────
    [Fact]
    public void MarkAsShipped_ConfirmedOrder_ShouldSucceed()
    {
        var order = Order.Create(ValidCustomerId).Value;
        order.AddItem(ValidProductId, "Product A", ValidMoney, 1);
        order.Confirm();

        var result = order.MarkAsShipped();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void MarkAsShipped_PendingOrder_ShouldReturnFailure()
    {
        var order = Order.Create(ValidCustomerId).Value;

        var result = order.MarkAsShipped();

        result.IsFailure.Should().BeTrue();
    }
}