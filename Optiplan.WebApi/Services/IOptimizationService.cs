using Optiplan.DatabaseResources;
using Optiplan.WebApi.DataTransferObjects;

namespace Optiplan.WebApi.Services;

public interface IOptimizationService
{
    Task<WorkOrder[]> OptimizeByPartsAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList);
    Task<WorkOrder[]> OptimizeByCostsAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList);
    Task<WorkOrder[]> OptimizeBySafetyAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList);
}