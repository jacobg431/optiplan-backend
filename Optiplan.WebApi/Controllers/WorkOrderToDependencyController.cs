using Microsoft.AspNetCore.Mvc;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Repositories;

namespace Optiplan.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkOrderToDependencyController : ControllerBase
{
    private readonly IWorkOrderToDependencyRepository _repository;

    public WorkOrderToDependencyController(IWorkOrderToDependencyRepository repository)
    {
        _repository = repository;
    }

    // GET: api/workordertodependencies
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<WorkOrderToDependency>))]
    public async Task<IEnumerable<WorkOrderToDependency>> GetWorkOrderToDependencies()
    {
        return await _repository.RetrieveAllAsync();
    }

    // GET: api/workordertodependencies/[id]
    [HttpGet("{id}", Name = nameof(GetWorkOrderToDependency))] // Named in order to be referenced by other endpoints
    [ProducesResponseType(200, Type = typeof(WorkOrderToDependency))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWorkOrderToDependency(int id)
    {
        WorkOrderToDependency? w = await _repository.RetrieveAsync(id);
        if (w == null)
        {
            return NotFound();
        }
        return Ok(w);
    }

    // POST: api/workordertodependencies/[id]
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(WorkOrderToDependency))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] WorkOrderToDependency w)
    {
        if (w == null)
        {
            return BadRequest("No work order specified.");
        }
        
        WorkOrderToDependency? addedWorkOrderToDependency = await _repository.CreateAsync(w);
        if (addedWorkOrderToDependency == null)
        {
            return BadRequest("Repository failed to create work order.");
        }

        return CreatedAtRoute(
            routeName: nameof(GetWorkOrderToDependency), 
            routeValues: new { id = addedWorkOrderToDependency.DependencyInstanceId },
            value: addedWorkOrderToDependency
        );
    }

    // PUT: api/workordertodependencies/[id]
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] WorkOrderToDependency w)
    {
        if (w == null || w.DependencyInstanceId != id)
        {
            return BadRequest();
        }

        WorkOrderToDependency? existingWorkOrderToDependency = await _repository.RetrieveAsync(id);
        if (existingWorkOrderToDependency == null)
        {
            return NotFound();
        }

        await _repository.UpdateAsync(w);
        return new NoContentResult();
    }

    // DELETE: api/workordertodependencies/[id]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        WorkOrderToDependency? existingWorkOrderToDependency = await _repository.RetrieveAsync(id);
        if (existingWorkOrderToDependency == null)
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