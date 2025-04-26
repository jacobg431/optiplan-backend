using Microsoft.AspNetCore.Mvc;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Repositories;
using Optiplan.WebApi.Services;
using Optiplan.WebApi.DataTransferObjects;

namespace Optiplan.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OptimizationController : ControllerBase
{
    private readonly IDependencyRepository _dependencyRepository;
    private readonly IWorkOrderToDependencyRepository _workOrderToDependencyRepository;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IOptimizationService _optimizationService;

    public OptimizationController(
        IDependencyRepository dependencyRepository,
        IWorkOrderToDependencyRepository workOrderToDependencyRepository,
        IWorkOrderRepository workOrderRepository,
        IOptimizationService optimizationService
    )
    {
        _dependencyRepository = dependencyRepository;
        _workOrderToDependencyRepository = workOrderToDependencyRepository;
        _workOrderRepository = workOrderRepository;  
        _optimizationService = optimizationService;
    }

    // POST: api/optimization/parts
    [HttpPost("parts")]
    [ProducesResponseType(201, Type = typeof(IEnumerable<WorkOrder>))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public Task<ActionResult> OptimizeByParts([FromBody] IEnumerable<WorkOrderToDependencyDto> dtoList)
    {
        return OptimizeAsync(dtoList, _optimizationService.OptimizeByParts);
    }

    // POST: api/optimization/costs
    [HttpPost("costs")]
    [ProducesResponseType(201, Type = typeof(IEnumerable<WorkOrder>))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public Task<ActionResult> OptimizeByCosts([FromBody] IEnumerable<WorkOrderToDependencyDto> dtoList)
    {
        return OptimizeAsync(dtoList, _optimizationService.OptimizeByCosts);
    }

    // POST: api/optimization/safety
    [HttpPost("safety")]
    [ProducesResponseType(201, Type = typeof(IEnumerable<WorkOrder>))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public Task<ActionResult> OptimizeBySafety([FromBody] IEnumerable<WorkOrderToDependencyDto> dtoList)
    {
        return OptimizeAsync(dtoList, _optimizationService.OptimizeBySafety);
    }


    // Private methods

    private async Task<ActionResult> DenormalizeData(IEnumerable<WorkOrderToDependency> workOrderToDependencies)
    {
        if (workOrderToDependencies == null)
        {
            return BadRequest("No data in request");
        }

        IEnumerable<WorkOrder> workOrders = await _workOrderRepository.RetrieveAllAsync();
        IEnumerable<Dependency> dependencies = await _dependencyRepository.RetrieveAllAsync();
        IEnumerable<CustomWorkOrderDependencyDto> dtoList;

        try
        {
            dtoList = CustomWorkOrderDependencyMapper
                .ToDtoList(dependencies, workOrders, workOrderToDependencies);
        }
        catch(Exception e)
        {
            return BadRequest(e.Message);
        }

        int expectedNumberOfWorkOrders = dtoList.Select(r => r.WorkOrderId).Distinct().Count();

        return Ok(new {
            Data = dtoList, 
            ExpectedCount = expectedNumberOfWorkOrders
        });
    }

    private async Task<ActionResult> OptimizeAsync(
        IEnumerable<WorkOrderToDependencyDto> dtoList,
        Func<IEnumerable<CustomWorkOrderDependencyDto>, WorkOrder[]> optimizationMethod
    )
    {
        IEnumerable<WorkOrderToDependency> wList = dtoList.Select(WorkOrderToDependencyMapper.ToEntity);

        var denormalizedResult = await DenormalizeData(wList);
        if (denormalizedResult is not OkObjectResult)
        {
            return denormalizedResult;
        }

        var resultValue = ((dynamic)(OkObjectResult)denormalizedResult).Value;
        IEnumerable<CustomWorkOrderDependencyDto> resultDtoList = resultValue.Data;
        int expectedCount = resultValue.ExpectedCount;

        IEnumerable<WorkOrder> workOrdersToReturn = optimizationMethod(resultDtoList);
        if (!workOrdersToReturn.Any())
        {
            return StatusCode(500, "Error optimizing work orders");
        }
        
        int workOrdersReturned = workOrdersToReturn.Count(); 
        if (workOrdersReturned != expectedCount)
        {
            return StatusCode(500, $"Client requested to optimize {expectedCount} work orders, while server returned {workOrdersReturned}");
        }

        return StatusCode(201, value: workOrdersToReturn);

    }
}