using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.Repositories;

public interface IWorkOrderRepository
{
    Task<WorkOrder?> CreateAsync(WorkOrder c);
    Task<WorkOrder[]> RetrieveAllAsync();
    Task<WorkOrder?> RetrieveAsync(int id);
    Task<WorkOrder?> UpdateAsync(WorkOrder c);
    Task<bool?> DeleteAsync(int id);
}