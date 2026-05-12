using FluentAssertions;
using UnderworldAPI.Sales.Domain.Aggregates.Order;
using UnderworldAPI.Sales.Domain.ValueObjects;

namespace UnderworldAPI.Sales.Domain.Tests.Orders;

public class OrderItemTests
{
    private static readonly Money ValidMoney = Money.Create(100m, "MXN").Value;

    [Fact]
    public void AddItem_WithEmptyProductName_ShouldReturnFailure()
    {
        var order = Order.Create(Guid.NewGuid()).Value;

        var result = order.AddItem(Guid.NewGuid(), "", ValidMoney, 1);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ShouldReturnFailure()
    {
        var order = Order.Create(Guid.NewGuid()).Value;

        var result = order.AddItem(Guid.NewGuid(), "Product A", ValidMoney, 0);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddItem_WithNegativeQuantity_ShouldReturnFailure()
    {
        var order = Order.Create(Guid.NewGuid()).Value;

        var result = order.AddItem(Guid.NewGuid(), "Product A", ValidMoney, -1);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Total_ShouldBeUnitPriceTimesQuantity()
    {
        var order = Order.Create(Guid.NewGuid()).Value;
        var money = Money.Create(50m, "MXN").Value;
        order.AddItem(Guid.NewGuid(), "Product A", money, 3);

        order.Items[0].Total.Amount.Should().Be(150m);
    }
}