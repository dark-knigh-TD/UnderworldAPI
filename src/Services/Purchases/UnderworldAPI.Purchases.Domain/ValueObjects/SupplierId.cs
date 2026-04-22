using System;
using UnderworldAPI.Shared.Domain.Primitives;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Domain.ValueObjects;

public sealed class SupplierId : ValueObject
{ 
    public Guid Value { get; private set; }
    private SupplierId() { }
    private SupplierId(Guid value)=>
        Value = value;

    public static Result<SupplierId> Create(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<SupplierId>(
                Error.Validation(nameof(Value), "SupplierId cannot be empty."));

        return Result.Success(new SupplierId(value));
    }
       
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
    public override string ToString() => Value.ToString();
}
