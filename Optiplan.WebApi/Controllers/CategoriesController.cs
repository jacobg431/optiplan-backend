using Microsoft.AspNetCore.Mvc;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Repositories;

namespace Optiplan.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _repository;

    public CategoriesController(ICategoryRepository repository)
    {
        _repository = repository;
    }

    // GET: api/categories
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Category>))]
    public async Task<IEnumerable<Category>> GetCategories()
    {
        return await _repository.RetrieveAllAsync();
    }

    // GET: api/categories/[id]
    [HttpGet("{id}", Name = nameof(GetCategory))] // Named in order to be referenced by other endpoints
    [ProducesResponseType(200, Type = typeof(Category))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCategory(int id)
    {
        Category? c = await _repository.RetrieveAsync(id);
        if (c == null)
        {
            return NotFound();
        }
        return Ok(c);
    }

    // POST: api/categories/[id]
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(Category))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Category c)
    {
        if (c == null)
        {
            return BadRequest("No category specified.");
        }
        
        Category? addedCategory = await _repository.CreateAsync(c);
        if (addedCategory == null)
        {
            return BadRequest("Repository failed to create category.");
        }

        return CreatedAtRoute(
            routeName: nameof(GetCategory), 
            routeValues: new { id = addedCategory.Id },
            value: addedCategory
        );
    }

    // PUT: api/categories/[id]
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] Category c)
    {
        if (c == null || c.Id != id)
        {
            return BadRequest();
        }

        Category? existingCategory = await _repository.RetrieveAsync(id);
        if (existingCategory == null)
        {
            return NotFound();
        }

        await _repository.UpdateAsync(c);
        return new NoContentResult();
    }

    // DELETE: api/categories/[id]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        Category? existingCategory = await _repository.RetrieveAsync(id);
        if (existingCategory == null)
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