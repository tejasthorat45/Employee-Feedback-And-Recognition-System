using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service) => _service = service;

    [HttpGet]
    [AllowAnonymous] // Allow all users to see departments
    public async Task<ActionResult<List<DepartmentReadDto>>> GetAll(CancellationToken ct)
    {
        var departments = await _service.GetAllAsync(ct);
        return Ok(departments);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<DepartmentReadDto>> GetById(string id, CancellationToken ct)
    {
        var dept = await _service.GetByIdAsync(id, ct);
        if (dept is null) return NotFound(new { message = "Department not found." });
        return Ok(dept);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentReadDto>> Create(DepartmentCreateDto dto, CancellationToken ct)
    {
        try
        {
            var created = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.DepartmentId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, DepartmentUpdateDto dto, CancellationToken ct)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto, ct);
            if (!updated) return NotFound(new { message = "Department not found." });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id, ct);
            if (!deleted) return NotFound(new { message = "Department not found." });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
