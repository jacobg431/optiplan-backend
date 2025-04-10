using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.Repositories;

public interface IDependencyRepository
{
    Task<Dependency?> CreateAsync(Dependency c);
    Task<Dependency[]> RetrieveAllAsync();
    Task<Dependency?> RetrieveAsync(int id);
    Task<Dependency?> UpdateAsync(Dependency c);
    Task<bool?> DeleteAsync(int id);
}