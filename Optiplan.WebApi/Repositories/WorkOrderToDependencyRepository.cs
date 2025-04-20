using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.Repositories;

public class WorkOrderToDependencyRepository : IWorkOrderToDependencyRepository
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions = new() 
    {
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };
    private OptiplanContext _dbContext;

    public WorkOrderToDependencyRepository(OptiplanContext dbContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public async Task<WorkOrderToDependency?> CreateAsync(WorkOrderToDependency w)
    {
        EntityEntry<WorkOrderToDependency> added = await _dbContext.WorkOrderToDependencies.AddAsync(w);
        int affected = await _dbContext.SaveChangesAsync();
        if (affected == 1) {
            _memoryCache.Set(w.DependencyInstanceId, w, _cacheEntryOptions);
            return w;
        }
        return null;
    }

    public Task<WorkOrderToDependency[]> RetrieveAllAsync()
    {
        return _dbContext.WorkOrderToDependencies.ToArrayAsync();
    }

    public Task<WorkOrderToDependency[]> RetrieveByWorkOrderId(int id)
    {
        return _dbContext.WorkOrderToDependencies.Where(w => w.WorkOrderId == id).ToArrayAsync();
    }

    public Task<WorkOrderToDependency[]> RetrieveByDependencyId(int id)
    {
        return _dbContext.WorkOrderToDependencies.Where(w => w.DependencyId == id).ToArrayAsync();
    }

    public async Task<WorkOrderToDependency?> RetrieveAsync(int id)
    {
        // Try to get from cache first
        if (_memoryCache.TryGetValue(id, out WorkOrderToDependency? wFromCache))
        {
            return wFromCache;
        }

        // Try to get from context change tracker or database
        WorkOrderToDependency? wFromDb = await _dbContext.WorkOrderToDependencies.FindAsync(id);
        if (wFromDb is null) 
        {
            return null;
        }

        _memoryCache.Set(wFromDb.DependencyInstanceId, wFromDb, _cacheEntryOptions);
        return wFromDb;
    }

    public async Task<WorkOrderToDependency?> UpdateAsync(WorkOrderToDependency w)
    {
        _dbContext.WorkOrderToDependencies.Update(w);
        int affected = await _dbContext.SaveChangesAsync();
        if (affected == 1) {
            _memoryCache.Set(w.DependencyInstanceId, w, _cacheEntryOptions);
            return w;
        }
        return null;
    }

    public async Task<bool?> DeleteAsync(int id)
    {
        WorkOrderToDependency? workOrderToDependency = await _dbContext.WorkOrderToDependencies.FindAsync(id);
        if (workOrderToDependency is null) 
        {
            return null;
        }

        int affected = await _dbContext.SaveChangesAsync();
        if (affected == 1)
        {
            _memoryCache.Remove(workOrderToDependency.DependencyInstanceId);
            return true;
        }

        return false;
    }
}