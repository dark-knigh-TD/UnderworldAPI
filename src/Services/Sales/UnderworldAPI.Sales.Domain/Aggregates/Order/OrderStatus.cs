using System;

namespace UnderworldAPI.Sales.Domain.Aggregates.Order;

public enum OrderStatus
{
    Pending   = 1,  // Creada, sin confirmar
    Confirmed = 2,  // Confirmada por el sistema/usuario
    Shipped   = 3,  // Enviada al cliente
    Delivered = 4,  // Entregada exitosamente
    Cancelled = 5   // Cancelada — estado terminal
}
