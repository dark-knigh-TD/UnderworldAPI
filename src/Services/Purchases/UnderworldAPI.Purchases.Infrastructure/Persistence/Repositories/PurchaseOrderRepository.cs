using System;
using Microsoft.EntityFrameworkCore;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;
using UnderworldAPI.Purchases.Domain.Ports.Repositories;

namespace UnderworldAPI.Purchases.Infrastructure.Persistence.Repositories;

internal sealed class PurchaseOrderRepository(
    PurchasesDbContext dbContext
) : IPurchaseOrderRepository
{

    public async Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)=>
        await dbContext.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);
    

    public async Task<IEnumerable<PurchaseOrder>> GetAllAsync(CancellationToken cancellationToken = default)=>
        await dbContext.PurchaseOrders
            .Include(po => po.Items)
            .ToListAsync(cancellationToken);
   
    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)=>
        await dbContext.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
        
    public Task UpdateAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        dbContext.PurchaseOrders.Update(purchaseOrder);
        return Task.CompletedTask;
    }
    public Task DeleteAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        dbContext.PurchaseOrders.Remove(purchaseOrder);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)=>
        await dbContext.PurchaseOrders
        .AsNoTracking().AnyAsync(po => po.Id == id, cancellationToken);
    

    

    

   
}
