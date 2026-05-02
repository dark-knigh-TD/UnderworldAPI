using System;

namespace UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;

public enum PurchaseOrderStatus
{
    Draft     = 1,  // Borrador — aún no enviada al proveedor
    Sent      = 2,  // Enviada al proveedor
    Confirmed = 3,  // Confirmada por el proveedor
    Received  = 4,  // Mercancía recibida en almacén
    Cancelled = 5   // Cancelada — estado terminal
}
