using System;
using UnderworldAPI.Purchases.Domain.ValueObjects;
using UnderworldAPI.Shared.Domain.Primitives;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;

public sealed class PurchaseOrderItem:Entity
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public int? ReceivedQuantity { get; private set; }

    public Money TotalPrice => UnitPrice.Multiply(Quantity);

    private PurchaseOrderItem() { }

    public PurchaseOrderItem(Guid id, Guid purchaseOrderId, Guid productId, string productName, Money unitPrice, int quantity)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty.", nameof(productName));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        Quantity = quantity;
    }

    internal static Result<PurchaseOrderItem> Create(Guid purchaseOrderId, Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
            return Result.Failure<PurchaseOrderItem>(
                Error.Validation(nameof(productName), "Product name cannot be empty."));

        if (quantity <= 0)
            return Result.Failure<PurchaseOrderItem>(
                Error.Validation(nameof(quantity), "Quantity must be greater than zero."));

        if (unitPrice is null)
            return Result.Failure<PurchaseOrderItem>(
                Error.Validation(nameof(unitPrice), "Unit price cannot be null."));

        return Result.Success(new PurchaseOrderItem(Guid.NewGuid(), purchaseOrderId, productId,
         productName, unitPrice, quantity));    
    }

    internal void ReceiveQuantity(int receivedQuantity)
    {
        if (receivedQuantity <= 0)
            throw new ArgumentException("Received quantity must be greater than zero.", nameof(receivedQuantity));

        if (ReceivedQuantity.HasValue && ReceivedQuantity.Value + receivedQuantity > Quantity)
            throw new InvalidOperationException("Cannot receive more than the ordered quantity.");

        ReceivedQuantity = (ReceivedQuantity ?? 0) + receivedQuantity;
    }
}
