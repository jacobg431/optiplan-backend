using Optiplan.WebApi.Repositories;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.DataTransferObjects;

namespace Optiplan.WebApi.Services;

public class OptimizationService : IOptimizationService
{
    private readonly IWorkOrderRepository _workOrderRepository;

    public OptimizationService(IWorkOrderRepository workOrderRepository)
    {
        _workOrderRepository = workOrderRepository;
    }

    
    public async Task<WorkOrder[]> OptimizeByPartsAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return await _workOrderRepository.RetrieveAllAsync(); // Placeholder for now ...
    }
    public async Task<WorkOrder[]> OptimizeByCostsAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return await _workOrderRepository.RetrieveAllAsync();
    }
    public async Task<WorkOrder[]> OptimizeBySafetyAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return await _workOrderRepository.RetrieveAllAsync();
    }

    //private async Task<WorkOrder[]> DateTimeRandomizerAsync(){}

}