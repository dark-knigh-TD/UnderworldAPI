using System;

namespace UnderworldAPI.Purchases.Domain.Exceptions;

public sealed class PurchaseOrderDomainException:Exception
{
    public PurchaseOrderDomainException(string message) : base(message) { }

    public PurchaseOrderDomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
