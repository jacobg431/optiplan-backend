using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.Repositories;

public interface ICategoryRepository 
{
    Task<Category?> CreateAsync(Category c);
    Task<Category[]> RetrieveAllAsync();
    Task<Category?> RetrieveAsync(int id);
    Task<Category?> UpdateAsync(Category c);
    Task<bool?> DeleteAsync(int id);
}