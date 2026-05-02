using FluentAssertions;
using UnderworldAPI.Sales.Domain.Aggregates.Order;

namespace UnderworldAPI.Sales.Domain.Tests.Orders;

public class OrderTests
{
    [Fact]
    public void Create_WithValidCustomerId_ShouldReturnSuccess()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var result = Order.Create(customerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(OrderStatus.Pending);
        result.Value.CustomerId.Value.Should().Be(customerId);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldReturnFailure()
    {
        // Arrange
        var customerId = Guid.Empty;

        // Act
        var result = Order.Create(customerId);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}