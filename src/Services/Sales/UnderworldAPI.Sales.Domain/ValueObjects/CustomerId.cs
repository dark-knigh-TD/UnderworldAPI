using UnderworldAPI.Shared.Domain.Results;
using UnderworldAPI.Shared.Domain.Primitives;


namespace UnderworldAPI.Sales.Domain.ValueObjects;

public sealed class CustomerId : ValueObject
{
    public Guid Value { get; private set; }

    private CustomerId() { }
    private CustomerId(Guid value)=> Value= value;
    

    public static Result<CustomerId>  Create(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<CustomerId>(
                Error.Validation(nameof(CustomerId), "CustomerId cannot be empty."));

        return Result.Success(new CustomerId(value));
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}
