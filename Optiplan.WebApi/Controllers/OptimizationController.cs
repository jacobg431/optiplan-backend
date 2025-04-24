using Microsoft.AspNetCore.Mvc;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Repositories;
using Optiplan.WebApi.Services;
using Optiplan.WebApi.DataTransferObjects;
using System.Diagnostics;

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
        return OptimizeAsync(dtoList, _optimizationService.OptimizeByPartsAsync);
    }

    // POST: api/optimization/costs
    [HttpPost("costs")]
    [ProducesResponseType(201, Type = typeof(IEnumerable<WorkOrder>))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public Task<ActionResult> OptimizeByCosts([FromBody] IEnumerable<WorkOrderToDependencyDto> dtoList)
    {
        return OptimizeAsync(dtoList, _optimizationService.OptimizeByCostsAsync);
    }

    // POST: api/optimization/safety
    [HttpPost("safety")]
    [ProducesResponseType(201, Type = typeof(IEnumerable<WorkOrder>))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public Task<ActionResult> OptimizeBySafety([FromBody] IEnumerable<WorkOrderToDependencyDto> dtoList)
    {
        return OptimizeAsync(dtoList, _optimizationService.OptimizeBySafetyAsync);
    }


    // Private methods

    private async Task<ActionResult> DenormalizeData(IEnumerable<WorkOrderToDependency> wList)
    {
        if (wList == null || !wList.Any())
        {
            return BadRequest("No data in request");
        }

        IEnumerable<WorkOrder> workOrders = await _workOrderRepository.RetrieveAllAsync();
        IEnumerable<Dependency> dependencies = await _dependencyRepository.RetrieveAllAsync();

        var resultFirstJoin = wList.Join(workOrders, wotd => wotd.WorkOrderId, wo => wo.Id, (wotd, wo) => new {
            wotd.DependencyInstanceId,
            wotd.WorkOrderId,
            wotd.DependencyId,
            WorkOrderName = wo.Name,
            WorkOrderStart = wo.StartDateTime,
            WorkOrderStop = wo.StopDateTime,
            wotd.TextAttributeValue,
            wotd.IntegerAttributeValue,
            wotd.NumberAttributeValue,
            wotd.BooleanAttributeValue,
            DependencyStart = wotd.StartDateTime,
            DependencyStop = wotd.StopDateTime
        });

        int expectedNumberOfWorkOrders = resultFirstJoin.Select(r => r.WorkOrderId).Distinct().Count();
        if (expectedNumberOfWorkOrders == 0)
        {
            return BadRequest("Request not containing references to valid work orders");
        }

        var resultSecondJoin = resultFirstJoin.Join(dependencies, r => r.DependencyId, d => d.Id, (r, d) => new {
            r.DependencyInstanceId,
            r.WorkOrderId,
            r.DependencyId,
            r.WorkOrderName,
            r.WorkOrderStart,
            r.WorkOrderStop,
            r.TextAttributeValue,
            r.IntegerAttributeValue,
            r.NumberAttributeValue,
            r.BooleanAttributeValue,
            r.DependencyStart,
            r.DependencyStop,
            DependencyName = d.Name
        });

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = resultSecondJoin.Select(r => new CustomWorkOrderDependencyDto {
            DependencyInstanceId = r.DependencyInstanceId,
            WorkOrderId = r.WorkOrderId,
            DependencyId = r.DependencyId,
            WorkOrderName = r.WorkOrderName,
            WorkOrderStart = r.WorkOrderStart,
            WorkOrderStop = r.WorkOrderStop,
            TextAttributeValue = r.TextAttributeValue,
            IntegerAttributeValue = r.IntegerAttributeValue,
            NumberAttributeValue = r.NumberAttributeValue,
            BooleanAttributeValue = r.BooleanAttributeValue,
            DependencyStart = r.DependencyStart,
            DependencyStop = r.DependencyStop,
            DependencyName = r.DependencyName
        });

        return Ok(new {
            Data = dtoList, 
            ExpectedCount = expectedNumberOfWorkOrders
        });

    }

    private async Task<ActionResult> OptimizeAsync(
        IEnumerable<WorkOrderToDependencyDto> dtoList,
        Func<IEnumerable<CustomWorkOrderDependencyDto>, Task<WorkOrder[]>> optimizationMethod
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

        IEnumerable<WorkOrder> workOrdersToReturn = await optimizationMethod(resultDtoList);
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