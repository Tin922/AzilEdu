using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using AzilEdu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VolunteersController : ControllerBase
{
    private readonly AzilEduDbContext _context;
    public VolunteersController(AzilEduDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<List<VolunteerDto>>> GetAll() =>
        Ok(await _context.Volunteers.Include(v => v.VolunteerStatus).OrderBy(v => v.LastName).ThenBy(v => v.FirstName)
            .Select(v => new VolunteerDto
            {
                Id = v.Id, FirstName = v.FirstName, LastName = v.LastName, Email = v.Email, Phone = v.Phone,
                Skills = v.Skills, AvailableFrom = v.AvailableFrom, Notes = v.Notes,
                VolunteerStatusId = v.VolunteerStatusId, Status = v.VolunteerStatus!.Name
            }).ToListAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<VolunteerDto>> GetById(int id)
    {
        var v = await _context.Volunteers.Include(x => x.VolunteerStatus).FirstOrDefaultAsync(x => x.Id == id);
        if (v is null) return NotFound();
        return Ok(new VolunteerDto
        {
            Id = v.Id, FirstName = v.FirstName, LastName = v.LastName, Email = v.Email, Phone = v.Phone,
            Skills = v.Skills, AvailableFrom = v.AvailableFrom, Notes = v.Notes,
            VolunteerStatusId = v.VolunteerStatusId, Status = v.VolunteerStatus!.Name
        });
    }

    [HttpPost]
    public async Task<ActionResult<VolunteerDto>> Create(SaveVolunteerDto dto)
    {
        var v = new Volunteer
        {
            FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email, Phone = dto.Phone,
            Skills = dto.Skills, AvailableFrom = dto.AvailableFrom, Notes = dto.Notes, VolunteerStatusId = dto.VolunteerStatusId
        };
        _context.Volunteers.Add(v);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = v.Id }, (await GetById(v.Id)).Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, SaveVolunteerDto dto)
    {
        var v = await _context.Volunteers.FindAsync(id);
        if (v is null) return NotFound();
        v.FirstName = dto.FirstName; v.LastName = dto.LastName; v.Email = dto.Email; v.Phone = dto.Phone;
        v.Skills = dto.Skills; v.AvailableFrom = dto.AvailableFrom; v.Notes = dto.Notes; v.VolunteerStatusId = dto.VolunteerStatusId;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var v = await _context.Volunteers.FindAsync(id);
        if (v is null) return NotFound();
        _context.Volunteers.Remove(v);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
