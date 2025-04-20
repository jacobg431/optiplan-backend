using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.Repositories;

public class DependencyRepository : IDependencyRepository
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions = new() 
    {
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };
    private OptiplanContext _dbContext;

    public DependencyRepository(OptiplanContext dbContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public async Task<Dependency?> CreateAsync(Dependency d)
    {
        EntityEntry<Dependency> added = await _dbContext.Dependencies.AddAsync(d);
        int affected = await _dbContext.SaveChangesAsync();
        if (affected == 1) {
            _memoryCache.Set(d.Id, d, _cacheEntryOptions);
            return d;
        }
        return null;
    }

    public Task<Dependency[]> RetrieveAllAsync()
    {
        return _dbContext.Dependencies.ToArrayAsync();
    }

    public async Task<Dependency?> RetrieveAsync(int id)
    {
        // Try to get from cache first
        if (_memoryCache.TryGetValue(id, out Dependency? dependencyFromCache))
        {
            return dependencyFromCache;
        }

        // Try to get from context change tracker or database
        Dependency? dependencyFromDb = await _dbContext.Dependencies.FindAsync(id);
        if (dependencyFromDb is null) 
        {
            return null;
        }

        _memoryCache.Set(dependencyFromDb.Id, dependencyFromDb, _cacheEntryOptions);
        return dependencyFromDb;
    }

    public async Task<Dependency?> UpdateAsync(Dependency d)
    {
        _dbContext.Dependencies.Update(d);
        int affected = await _dbContext.SaveChangesAsync();
        if (affected == 1) {
            _memoryCache.Set(d.Id, d, _cacheEntryOptions);
            return d;
        }
        return null;
    }

    public async Task<bool?> DeleteAsync(int id)
    {
        Dependency? dependency = await _dbContext.Dependencies.FindAsync(id);
        if (dependency is null) 
        {
            return null;
        }

        int affected = await _dbContext.SaveChangesAsync();
        if (affected == 1)
        {
            _memoryCache.Remove(dependency.Id);
            return true;
        }

        return false;
    }
}