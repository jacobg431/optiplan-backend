using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.Repositories;

public class WorkOrderRepository : IWorkOrderRepository
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions = new() 
    {
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };
    private OptiplanContext _dbContext;

    public WorkOrderRepository(OptiplanContext dbContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public async Task<WorkOrder?> CreateAsync(WorkOrder w)
    {
        EntityEntry<WorkOrder> added = await _dbContext.WorkOrders.AddAsync(w);
        int affected = await _dbContext.SaveChangesAsync();
        if (affected == 1) {
            _memoryCache.Set(w.Id, w, _cacheEntryOptions);
            return w;
        }
        return null;
    }

    public Task<WorkOrder[]> RetrieveAllAsync()
    {
        return _dbContext.WorkOrders.ToArrayAsync();
    }

    public async Task<WorkOrder?> RetrieveAsync(int id)
    {
        // Try to get from cache first
        if (_memoryCache.TryGetValue(id, out WorkOrder? workOrderFromCache))
        {
            return workOrderFromCache;
        }

        // Try to get from context change tracker or database
        WorkOrder? workOrderFromDb = await _dbContext.WorkOrders.FindAsync(id);
        if (workOrderFromDb is null) 
        {
            return null;
        }

        _memoryCache.Set(workOrderFromDb.Id, workOrderFromDb, _cacheEntryOptions);
        return workOrderFromDb;
    }

    public async Task<WorkOrder?> UpdateAsync(WorkOrder w)
    {
        _dbContext.WorkOrders.Update(w);
        int affected = await _dbContext.SaveChangesAsync();
        if (affected == 1) {
            _memoryCache.Set(w.Id, w, _cacheEntryOptions);
            return w;
        }
        return null;
    }

    public async Task<bool?> DeleteAsync(int id)
    {
        WorkOrder? workOrder = await _dbContext.WorkOrders.FindAsync(id);
        if (workOrder is null) 
        {
            return null;
        }

        int affected = await _dbContext.SaveChangesAsync();
        if (affected == 1)
        {
            _memoryCache.Remove(workOrder.Id);
            return true;
        }

        return false;
    }
}