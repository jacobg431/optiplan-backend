using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions = new() 
    {
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };
    private OptiplanContext _dbContext;

    public CategoryRepository(OptiplanContext dbContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public async Task<Category?> CreateAsync(Category c)
    {
        EntityEntry<Category> added = await _dbContext.Categories.AddAsync(c);
        int affected = await _dbContext.SaveChangesAsync();
        if (affected == 1) {
            _memoryCache.Set(c.Id, c, _cacheEntryOptions);
            return c;
        }
        return null;
    }

    public Task<Category[]> RetrieveAllAsync()
    {
        return _dbContext.Categories.ToArrayAsync();
    }

    public async Task<Category?> RetrieveAsync(int id)
    {
        // Try to get from cache first
        if (_memoryCache.TryGetValue(id, out Category? categoryFromCache))
        {
            return categoryFromCache;
        }

        // Try to get from context change tracker or database
        Category? categoryFromDb = await _dbContext.Categories.FindAsync(id);
        if (categoryFromDb is null) 
        {
            return null;
        }

        _memoryCache.Set(categoryFromDb.Id, categoryFromDb, _cacheEntryOptions);
        return categoryFromDb;
    }

    public async Task<Category?> UpdateAsync(Category c)
    {
        _dbContext.Categories.Update(c);
        int affected = await _dbContext.SaveChangesAsync();
        if (affected == 1) {
            _memoryCache.Set(c.Id, c, _cacheEntryOptions);
            return c;
        }
        return null;
    }

    public async Task<bool?> DeleteAsync(int id)
    {
        Category? category = await _dbContext.Categories.FindAsync(id);
        if (category is null) 
        {
            return null;
        }

        int affected = await _dbContext.SaveChangesAsync();
        if (affected == 1)
        {
            _memoryCache.Remove(category.Id);
            return true;
        }

        return false;
    }
}