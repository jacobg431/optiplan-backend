using Microsoft.AspNetCore.Mvc;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Repositories;

namespace Optiplan.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkOrderController : ControllerBase
{
    private readonly IWorkOrderRepository _repository;

    public WorkOrderController(IWorkOrderRepository repository)
    {
        _repository = repository;
    }

    // GET: api/workorders
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<WorkOrder>))]
    public async Task<IEnumerable<WorkOrder>> GetWorkOrders()
    {
        return await _repository.RetrieveAllAsync();
    }

    // GET: api/workorders/[id]
    [HttpGet("{id}", Name = nameof(GetWorkOrder))] // Named in order to be referenced by other endpoints
    [ProducesResponseType(200, Type = typeof(WorkOrder))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWorkOrder(int id)
    {
        WorkOrder? w = await _repository.RetrieveAsync(id);
        if (w == null)
        {
            return NotFound();
        }
        return Ok(w);
    }

    // POST: api/workorders/[id]
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(WorkOrder))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] WorkOrder w)
    {
        if (w == null)
        {
            return BadRequest("No work order specified.");
        }
        
        WorkOrder? addedWorkOrder = await _repository.CreateAsync(w);
        if (addedWorkOrder == null)
        {
            return BadRequest("Repository failed to create work order.");
        }

        return CreatedAtRoute(
            routeName: nameof(GetWorkOrder), 
            routeValues: new { id = addedWorkOrder.Id },
            value: addedWorkOrder
        );
    }

    // PUT: api/workorders/[id]
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] WorkOrder w)
    {
        if (w == null || w.Id != id)
        {
            return BadRequest();
        }

        WorkOrder? existingWorkOrder = await _repository.RetrieveAsync(id);
        if (existingWorkOrder == null)
        {
            return NotFound();
        }

        await _repository.UpdateAsync(w);
        return new NoContentResult();
    }

    // DELETE: api/workorders/[id]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        WorkOrder? existingWorkOrder = await _repository.RetrieveAsync(id);
        if (existingWorkOrder == null)
        {
            return NotFound();
        }

        bool? deleted = await _repository.DeleteAsync(id);
        if (deleted == null) {
            return NotFound();
        }
        if (deleted is false)
        {
            return BadRequest();
        }

        return new NoContentResult();

    }

}