using UnderworldAPI.Shared.Domain.Primitives;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Domain.ValueObjects;

public class Money:ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

   public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return Result.Failure<Money>(
                Error.Validation(nameof(amount), "Amount cannot be negative."));

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Money>(         
                Error.Validation(nameof(currency), "Currency cannot be empty."));

         if (currency.Length != 3)
            return Result.Failure<Money>(
                Error.Validation(nameof(Currency), "Currency must be a 3-letter ISO code."));

        return Result.Success(new Money(amount, currency.ToUpperInvariant()));
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot add Money with different currencies: {Currency} and {other.Currency}.");
        
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));

        return new Money(Amount * quantity, Currency);
    }

    public static Money Zero(string currency = "MXN") => new(0, currency);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return (nameof(Amount), Amount);
        yield return (nameof(Currency), Currency);
    }

    public override string ToString() => $"{Amount} {Currency}";
}
