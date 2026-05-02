using System;
using Microsoft.EntityFrameworkCore;
using UnderworldAPI.Sales.Domain.Aggregates.Order;
using UnderworldAPI.Sales.Domain.Ports.Repositories;

namespace UnderworldAPI.Sales.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(SalesDbContext dbContext) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)=>
        await dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)=>
        await dbContext.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)=>
        await dbContext.Orders.AddAsync(order, cancellationToken);

    
    public  Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        // EF Core Change Tracker ya rastrea los cambios —
        // solo necesitamos marcar el estado explícitamente si viene desconectado
        dbContext.Orders.Update(order);
        return Task.CompletedTask;
    }
        
    public Task DeleteAsync(Order order, CancellationToken cancellationToken = default)
    {
        dbContext.Orders.Remove(order);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)=>
        await dbContext.Orders.AsNoTracking()
            .AnyAsync(o => o.Id == id, cancellationToken);
   

    
    

    
}
