using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonorTypesController : ControllerBase
{
    private readonly AzilEduDbContext _context;
    public DonorTypesController(AzilEduDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetAll() =>
        Ok(await _context.DonorTypes.OrderBy(t => t.Name).Select(t => new LookupDto { Id = t.Id, Name = t.Name }).ToListAsync());
}
