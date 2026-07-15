using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonorStatusesController : ControllerBase
{
    private readonly AzilEduDbContext _context;
    public DonorStatusesController(AzilEduDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetAll() =>
        Ok(await _context.DonorStatuses.OrderBy(s => s.Name).Select(s => new LookupDto { Id = s.Id, Name = s.Name }).ToListAsync());
}
