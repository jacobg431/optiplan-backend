using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.Repositories;

public interface IWorkOrderToDependencyRepository 
{
    Task<WorkOrderToDependency?> CreateAsync(WorkOrderToDependency w);
    Task<WorkOrderToDependency[]> RetrieveAllAsync();
    Task<WorkOrderToDependency?> RetrieveAsync(int id);
    Task<WorkOrderToDependency?> UpdateAsync(WorkOrderToDependency w);
    Task<bool?> DeleteAsync(int id);
}