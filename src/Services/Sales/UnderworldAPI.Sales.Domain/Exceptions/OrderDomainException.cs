using System;

namespace UnderworldAPI.Sales.Domain.Exceptions;

public sealed class OrderDomainException : Exception
{
    public OrderDomainException(string message) : base(message) { }

    public OrderDomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
