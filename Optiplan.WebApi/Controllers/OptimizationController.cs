using Microsoft.AspNetCore.Mvc;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Repositories;
using Optiplan.WebApi.Services;

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
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(IEnumerable<WorkOrder>))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> OptimizeByParts([FromBody] IEnumerable<WorkOrderToDependency> wList)
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
            BadRequest("Request not containing references to valid work orders");
        }

        var resultSecondJoin = resultFirstJoin.Join(dependencies, r => r.DependencyId, d => d.Id, (r, d) => new {
            r.DependencyInstanceId,
            r.WorkOrderId,
            r.DependencyId,
            r.WorkOrderStart,
            r.WorkOrderStop,
            r.TextAttributeValue,
            r.IntegerAttributeValue,
            r.NumberAttributeValue,
            r.BooleanAttributeValue,
            r.DependencyStart,
            r.DependencyStop,
            d.Name
        });

        IEnumerable<WorkOrder> workOrdersToReturn = await _optimizationService.OptimizeByPartsAsync(resultSecondJoin);
        if (!workOrdersToReturn.Any())
        {
            return StatusCode(500, "Error optimizing work orders.");
        }
        
        int workOrdersReturned = workOrdersToReturn.Count(); 
        if (workOrdersReturned != expectedNumberOfWorkOrders)
        {
            return StatusCode(500, $"Client requested to optimize {expectedNumberOfWorkOrders} work orders, while server returned {workOrdersReturned}");
        }

        return StatusCode(201, value: workOrdersToReturn);

    }

    // POST: api/optimization/costs

    // POST: api/optimization/safety
}