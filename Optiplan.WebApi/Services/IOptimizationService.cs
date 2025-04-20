using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.Services;

public interface IOptimizationService
{
    Task<WorkOrder[]> OptimizeByPartsAsync(object denormalizedWorkOrderDependencyObject);
    Task<WorkOrder[]> OptimizeByCostsAsync(object denormalizedWorkOrderDependencyObject);
    Task<WorkOrder[]> OptimizeBySafetyAsync(object denormalizedWorkOrderDependencyObject);
}