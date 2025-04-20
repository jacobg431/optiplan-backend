using Microsoft.AspNetCore.Mvc;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Repositories;

namespace Optiplan.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DependencyController : ControllerBase
{
    private readonly IDependencyRepository _repository;
    private readonly IWorkOrderToDependencyRepository _workOrderToDependencyRepository;
    private readonly IWorkOrderRepository _workOrderRepository;

    public DependencyController(
        IDependencyRepository repository,
        IWorkOrderToDependencyRepository workOrderToDependencyRepository,
        IWorkOrderRepository workOrderRepository
    )
    {
        _repository = repository;
        _workOrderToDependencyRepository = workOrderToDependencyRepository;
        _workOrderRepository = workOrderRepository;   
    }

    // GET: api/dependencies
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Dependency>))]
    public async Task<IEnumerable<Dependency>> GetDependencies()
    {
        return await _repository.RetrieveAllAsync();
    }

    // GET: api/dependencies/[id]
    [HttpGet("{id}", Name = nameof(GetDependency))] // Named in order to be referenced by other endpoints
    [ProducesResponseType(200, Type = typeof(Dependency))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetDependency(int id)
    {
        Dependency? d = await _repository.RetrieveAsync(id);
        if (d == null)
        {
            return NotFound();
        }
        return Ok(d);
    }

    // GET: api/dependencies/[id]/workorders
    [HttpGet("{id}/workorders")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetDependencyWorkOrders(int id)
    {
        Dependency? dependency = await _repository.RetrieveAsync(id);
        if (dependency == null)
        {
            return NotFound();
        }

        IEnumerable<WorkOrderToDependency> workOrderToDependencies = await _workOrderToDependencyRepository.RetrieveByDependencyId(id);
        IEnumerable<WorkOrder> workOrders = await _workOrderRepository.RetrieveAllAsync();
        var result = workOrderToDependencies.Join(workOrders, wotd => wotd.DependencyId, wo => wo.Id, (wotd, wo) => new {
            wotd.DependencyInstanceId,
            wo.Name,
            wo.StartDateTime,
            wo.StopDateTime
        });

        return Ok(result);
    }

    // POST: api/dependencies/[id]
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(Dependency))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Dependency d)
    {
        if (d == null)
        {
            return BadRequest("No dependency specified.");
        }
        
        Dependency? addedDependency = await _repository.CreateAsync(d);
        if (addedDependency == null)
        {
            return BadRequest("Repository failed to create dependency.");
        }

        return CreatedAtRoute(
            routeName: nameof(GetDependency), 
            routeValues: new { id = addedDependency.Id },
            value: addedDependency
        );
    }

    // PUT: api/dependencies/[id]
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] Dependency d)
    {
        if (d == null || d.Id != id)
        {
            return BadRequest();
        }

        Dependency? existingDependency = await _repository.RetrieveAsync(id);
        if (existingDependency == null)
        {
            return NotFound();
        }

        await _repository.UpdateAsync(d);
        return new NoContentResult();
    }

    // DELETE: api/dependencies/[id]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        Dependency? existingDependency = await _repository.RetrieveAsync(id);
        if (existingDependency == null)
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