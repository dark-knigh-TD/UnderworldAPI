using FluentAssertions;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;

namespace UnderworldAPI.Purchases.Domain.Tests.PurchaseOrders;

public class PurchaseOrderTests
{
    [Fact]
    public void Create_WithValidSupplierId_ShouldReturnSuccess()
    {
        // Arrange
        var supplierId = Guid.NewGuid();

        // Act
        var result = PurchaseOrder.Create(supplierId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(PurchaseOrderStatus.Draft);
        result.Value.SupplierId.Value.Should().Be(supplierId);
    }

    [Fact]
    public void Create_WithEmptySupplierId_ShouldReturnFailure()
    {
        // Arrange
        var supplierId = Guid.Empty;

        // Act
        var result = PurchaseOrder.Create(supplierId);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}