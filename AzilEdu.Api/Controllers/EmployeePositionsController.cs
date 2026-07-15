using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeePositionsController : ControllerBase
{
    private readonly AzilEduDbContext _context;
    public EmployeePositionsController(AzilEduDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetAll() =>
        Ok(await _context.EmployeePositions.OrderBy(p => p.Name).Select(p => new LookupDto { Id = p.Id, Name = p.Name }).ToListAsync());
}
