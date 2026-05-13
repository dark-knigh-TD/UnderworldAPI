using FluentAssertions;
using UnderworldAPI.Sales.Domain.ValueObjects;

namespace UnderworldAPI.Sales.Domain.Tests.Orders.ValueObjects;

public class CustomerIdTests
{
    [Fact]
    public void Create_WithValidGuid_ShouldReturnSuccess()
    {
        var id = Guid.NewGuid();

        var result = CustomerId.Create(id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(id);
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldReturnFailure()
    {
        var result = CustomerId.Create(Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation");
    }

    [Fact]
    public void TwoCustomerIdsWithSameGuid_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var customerId1 = CustomerId.Create(id).Value;
        var customerId2 = CustomerId.Create(id).Value;

        customerId1.Should().Be(customerId2);
    }
}