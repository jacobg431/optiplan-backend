using Optiplan.WebApi.Repositories;
using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.Services;

public class OptimizationService : IOptimizationService
{
    private readonly IWorkOrderRepository _workOrderRepository;

    public OptimizationService(IWorkOrderRepository workOrderRepository)
    {
        _workOrderRepository = workOrderRepository;
    }

    
    public async Task<WorkOrder[]> OptimizeByPartsAsync(object denormalizedWorkOrderDependencyObject)
    {
        return await _workOrderRepository.RetrieveAllAsync(); // Placeholder for now ...
    }
    public async Task<WorkOrder[]> OptimizeByCostsAsync(object denormalizedWorkOrderDependencyObject)
    {
        return await _workOrderRepository.RetrieveAllAsync();
    }
    public async Task<WorkOrder[]> OptimizeBySafetyAsync(object denormalizedWorkOrderDependencyObject)
    {
        return await _workOrderRepository.RetrieveAllAsync();
    }

}