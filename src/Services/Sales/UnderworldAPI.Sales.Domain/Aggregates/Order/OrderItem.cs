using System;
using UnderworldAPI.Sales.Domain.ValueObjects;
using UnderworldAPI.Shared.Domain.Primitives;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Domain.Aggregates.Order;

public sealed class OrderItem:Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    
    public Money Total => UnitPrice.Multiply(Quantity);

    // Constructores
    private OrderItem() { } // EF Core
    private OrderItem(Guid id, Guid orderId,Guid productId, string productName, 
          Money unitPrice, int quantity):base(id)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
       
    }

    internal static Result<OrderItem> Create(
        Guid orderId,
        Guid productId,
        string productName,
        Money unitPrice,
        int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
            return Result.Failure<OrderItem>(
                Error.Validation(nameof(ProductName), "Product name cannot be empty."));

        if (quantity <= 0)
            return Result.Failure<OrderItem>(
                Error.Validation(nameof(Quantity), "Quantity must be greater than zero."));

        return Result.Success(new OrderItem(
            Guid.NewGuid(),
            orderId,
            productId,
            productName,
            unitPrice,
            quantity));
    }

    // internal — solo Order puede llamar esto
    internal void IncreaseQuantity(int additional)
    {
        if (additional <= 0)
            throw new ArgumentException("Additional quantity must be greater than zero.");

        Quantity += additional;
    }
}
