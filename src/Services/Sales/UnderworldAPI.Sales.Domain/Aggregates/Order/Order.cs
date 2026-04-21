using System;
using UnderworldAPI.Shared.Domain.Primitives;

namespace UnderworldAPI.Sales.Domain.Aggregates.Order;

public sealed  class Order:AggregateRoot
{
    private readonly List<OrderItem> _items = [];
    
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public decimal TotalAmount { get; private set; }

    // Constructor
    public Order(Guid id, Guid customerId, DateTime orderDate, decimal totalAmount)
    {
        Id = id;
        CustomerId = customerId;
        OrderDate = orderDate;
        TotalAmount = totalAmount;
    }

    // Method to update the total amount
    public void UpdateTotalAmount(decimal newTotalAmount)
    {
        TotalAmount = newTotalAmount;
        // You can also add domain events here if needed
    }
}
