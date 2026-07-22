using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using AzilEdu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonorsController : ControllerBase
{
    private readonly AzilEduDbContext _context;

    public DonorsController(AzilEduDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<DonorDto>>> GetDonors()
    {
        // DonorId će kasnije biti povezan s prijavljenim korisnikom preko AppUserId.
        var donors = await _context.Donors
            .Include(donor => donor.DonorType)
            .Include(donor => donor.DonorStatus)
            .OrderBy(donor => donor.OrganizationName)
            .ThenBy(donor => donor.LastName)
            .ThenBy(donor => donor.FirstName)
            .ToListAsync();

        return Ok(donors.Select(ToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DonorDto>> GetDonorById(int id)
    {
        var donor = await _context.Donors
            .Include(d => d.DonorType)
            .Include(d => d.DonorStatus)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (donor is null)
            return NotFound();

        return Ok(ToDto(donor));
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetDonorsLookup()
    {
        var donors = await _context.Donors
            .OrderBy(donor => donor.OrganizationName)
            .ThenBy(donor => donor.LastName)
            .ThenBy(donor => donor.FirstName)
            .Select(donor => new LookupDto
            {
                Id = donor.Id,
                Name = donor.OrganizationName != ""
                    ? donor.OrganizationName
                    : donor.FirstName + " " + donor.LastName
            })
            .ToListAsync();

        return Ok(donors);
    }

    [HttpPost]
    public async Task<ActionResult<DonorDto>> CreateDonor(SaveDonorDto request)
    {
        if (request.DonorTypeId == 0 || request.DonorStatusId == 0)
            return BadRequest("Tip i status donatora su obavezni.");

        var donor = new Donor
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            OrganizationName = request.OrganizationName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            Address = request.Address.Trim(),
            City = request.City.Trim(),
            Notes = request.Notes.Trim(),
            CreatedAt = request.CreatedAt == default ? DateTime.Today : request.CreatedAt,
            DonorTypeId = request.DonorTypeId,
            DonorStatusId = request.DonorStatusId
        };

        _context.Donors.Add(donor);
        await _context.SaveChangesAsync();

        await _context.Entry(donor).Reference(d => d.DonorType).LoadAsync();
        await _context.Entry(donor).Reference(d => d.DonorStatus).LoadAsync();

        return CreatedAtAction(nameof(GetDonorById), new { id = donor.Id }, ToDto(donor));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDonor(int id, SaveDonorDto request)
    {
        var donor = await _context.Donors.FindAsync(id);

        if (donor is null)
            return NotFound();

        if (request.DonorTypeId == 0 || request.DonorStatusId == 0)
            return BadRequest("Tip i status donatora su obavezni.");

        donor.FirstName = request.FirstName.Trim();
        donor.LastName = request.LastName.Trim();
        donor.OrganizationName = request.OrganizationName.Trim();
        donor.Email = request.Email.Trim();
        donor.Phone = request.Phone.Trim();
        donor.Address = request.Address.Trim();
        donor.City = request.City.Trim();
        donor.Notes = request.Notes.Trim();
        donor.CreatedAt = request.CreatedAt == default ? donor.CreatedAt : request.CreatedAt;
        donor.DonorTypeId = request.DonorTypeId;
        donor.DonorStatusId = request.DonorStatusId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static DonorDto ToDto(Donor donor)
    {
        return new DonorDto
        {
            Id = donor.Id,
            FirstName = donor.FirstName,
            LastName = donor.LastName,
            OrganizationName = donor.OrganizationName,
            DisplayName = donor.OrganizationName != ""
                ? donor.OrganizationName
                : donor.FirstName + " " + donor.LastName,
            Email = donor.Email,
            Phone = donor.Phone,
            Address = donor.Address,
            City = donor.City,
            Notes = donor.Notes,
            CreatedAt = donor.CreatedAt,
            DonorTypeId = donor.DonorTypeId,
            Type = donor.DonorType?.Name ?? string.Empty,
            DonorStatusId = donor.DonorStatusId,
            Status = donor.DonorStatus?.Name ?? string.Empty
        };
    }
}
