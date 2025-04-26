using Optiplan.DatabaseResources;
using Optiplan.WebApi.DataTransferObjects;

namespace Optiplan.WebApi.Services;

public interface IOptimizationService
{
    WorkOrder[] OptimizeByParts(IEnumerable<CustomWorkOrderDependencyDto> dtoList);
    WorkOrder[] OptimizeByCosts(IEnumerable<CustomWorkOrderDependencyDto> dtoList);
    WorkOrder[] OptimizeBySafety(IEnumerable<CustomWorkOrderDependencyDto> dtoList);
}