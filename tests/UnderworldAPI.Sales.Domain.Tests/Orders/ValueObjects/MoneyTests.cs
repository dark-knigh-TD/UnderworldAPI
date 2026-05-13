using FluentAssertions;
using UnderworldAPI.Sales.Domain.ValueObjects;

namespace UnderworldAPI.Sales.Domain.Tests.Orders.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldReturnSuccess()
    {
        var result = Money.Create(100.00m, "MXN");

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(100.00m);
        result.Value.Currency.Should().Be("MXN");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldReturnFailure()
    {
        var result = Money.Create(-1m, "MXN");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation");
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldReturnFailure()
    {
        var result = Money.Create(100m, "");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithInvalidCurrencyLength_ShouldReturnFailure()
    {
        var result = Money.Create(100m, "MXNN");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnCorrectSum()
    {
        var money1 = Money.Create(100m, "MXN").Value;
        var money2 = Money.Create(200m, "MXN").Value;

        var result = money1.Add(money2);

        result.Amount.Should().Be(300m);
        result.Currency.Should().Be("MXN");
    }

    [Fact]
    public void Add_WithDifferentCurrencies_ShouldThrowException()
    {
        var money1 = Money.Create(100m, "MXN").Value;
        var money2 = Money.Create(100m, "USD").Value;

        var act = () => money1.Add(money2);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Multiply_WithPositiveQuantity_ShouldReturnCorrectResult()
    {
        var money = Money.Create(150m, "MXN").Value;

        var result = money.Multiply(3);

        result.Amount.Should().Be(450m);
    }

    [Fact]
    public void TwoMoneyWithSameValues_ShouldBeEqual()
    {
        var money1 = Money.Create(100m, "MXN").Value;
        var money2 = Money.Create(100m, "MXN").Value;

        money1.Should().Be(money2);
    }

    [Fact]
    public void TwoMoneyWithDifferentValues_ShouldNotBeEqual()
    {
        var money1 = Money.Create(100m, "MXN").Value;
        var money2 = Money.Create(200m, "MXN").Value;

        money1.Should().NotBe(money2);
    }
}