using System;
using UnderworldAPI.Purchases.Domain.Events;
using UnderworldAPI.Purchases.Domain.ValueObjects;
using UnderworldAPI.Shared.Domain.Primitives;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;

public sealed class PurchaseOrder:AggregateRoot
{
    private readonly List<PurchaseOrderItem> _items = [];

    public SupplierId SupplierId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

    public Money TotalAmount => _items.Aggregate(Money.Create(0, "USD").Value, (total, item) => total.Add(item.TotalPrice));

    //TODO: Si queremos soportar múltiples monedas, tendríamos que cambiar la propiedad TotalAmount para devolver un diccionario de moneda a monto total, o una lista de objetos que representen el total por moneda. Por ahora, asumimos que todas las líneas de pedido usan la misma moneda y devolvemos un solo total.
    // public Money TotalAmount => _items.Count == 0
    //     ? Money.Zero()
    //     : _items.Skip(1).Aggregate(
    //         _items[0].Total,
    //         (acc, item) => acc.Add(item.Total));

    private PurchaseOrder() { }
    private PurchaseOrder(Guid id, SupplierId supplierId, string? notes = null)
        : base(id)
    {
        SupplierId = supplierId;
        Status = PurchaseOrderStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        Notes = notes;
    }

    public static Result<PurchaseOrder> Create(Guid supplierId, string? notes = null)
    {
        var supplierIdResult = SupplierId.Create(supplierId);
        if (supplierIdResult.IsFailure)
            return Result.Failure<PurchaseOrder>(supplierIdResult.Error);

        var purchaseOrder = new PurchaseOrder(
            Guid.NewGuid(),
            supplierIdResult.Value,
            notes);

        purchaseOrder.RaiseDomainEvent(new PurchaseOrderCreatedEvent(
            purchaseOrder.Id,
            supplierIdResult.Value.Value,
            purchaseOrder.CreatedAt));

        return Result.Success(purchaseOrder);
    }

    public Result AddItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (Status != PurchaseOrderStatus.Draft)
            return Result.Failure(
                Error.Conflict(nameof(PurchaseOrder),
                 $"Items can only be added to Draft purchase orders. Current status: {Status}."));

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.ReceiveQuantity(quantity);
        }
        else
        {   

            var itemResult = PurchaseOrderItem.Create(Id, productId, productName, unitPrice, quantity);
            if (itemResult.IsFailure)
                return Result.Failure(itemResult.Error);

            _items.Add(itemResult.Value);
            
        }

        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new PurchaseOrderUpdatedEvent(Id, SupplierId.Value, UpdatedAt.Value));
        return Result.Success();
    }

    public Result Send()
    {
        if (Status != PurchaseOrderStatus.Draft)
            return Result.Failure(
                Error.Conflict(nameof(PurchaseOrder),
               $"Only Draft purchase orders can be sent. Current status: {Status}."));

        if (!_items.Any())
            return Result.Failure(
                Error.Validation(nameof(PurchaseOrder),
                 "Cannot send a purchase order without items."));

        Status = PurchaseOrderStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new PurchaseOrderUpdatedEvent(Id, SupplierId.Value, UpdatedAt.Value));
        return Result.Success();
    }

    public Result Confirm()
    {
        if (Status != PurchaseOrderStatus.Sent)
            return Result.Failure(
                Error.Conflict(nameof(PurchaseOrder),
               $"Only Sent purchase orders can be confirmed. Current status: {Status}."));

        Status = PurchaseOrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new PurchaseOrderUpdatedEvent(Id, SupplierId.Value, UpdatedAt.Value));
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == PurchaseOrderStatus.Received)
            return Result.Failure(
                Error.Conflict(nameof(PurchaseOrder),
                    "Cannot cancel a purchase order that has already been received."));

        if (Status == PurchaseOrderStatus.Cancelled)
            return Result.Failure(
                Error.Conflict(nameof(PurchaseOrder),
               "Purchase order is already cancelled."));

        Status = PurchaseOrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new PurchaseOrderUpdatedEvent(Id, SupplierId.Value, UpdatedAt.Value));
        return Result.Success();
    }   
}
