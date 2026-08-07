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
        var donors = await _context.Donors
            .Include(donor => donor.DonorType)
            .Include(donor => donor.DonorStatus)
            .OrderBy(donor => donor.LastName)
            .ThenBy(donor => donor.FirstName)
            .ToListAsync();

        return Ok(donors.Select(ToDto).ToList());
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetDonorsLookup()
    {
        var donors = await _context.Donors
            .OrderBy(donor => donor.LastName)
            .ThenBy(donor => donor.FirstName)
            .ToListAsync();

        var result = donors
            .Select(donor => new LookupDto
            {
                Id = donor.Id,
                Name = GetDisplayName(donor)
            })
            .ToList();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DonorDto>> GetDonorById(int id)
    {
        var donor = await _context.Donors
            .Include(item => item.DonorType)
            .Include(item => item.DonorStatus)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (donor is null)
            return NotFound();

        return Ok(ToDto(donor));
    }

    [HttpPost]
    public async Task<ActionResult<DonorDto>> CreateDonor(SaveDonorDto request)
    {
        var validationError = ValidateDonor(request);

        if (validationError is not null)
            return BadRequest(validationError);

        var donor = new Donor
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            OrganizationName = request.OrganizationName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            Notes = request.Notes,
            CreatedAt = request.CreatedAt,
            DonorTypeId = request.DonorTypeId,
            DonorStatusId = request.DonorStatusId
        };

        _context.Donors.Add(donor);
        await _context.SaveChangesAsync();

        await _context.Entry(donor).Reference(item => item.DonorType).LoadAsync();
        await _context.Entry(donor).Reference(item => item.DonorStatus).LoadAsync();

        return CreatedAtAction(nameof(GetDonorById), new { id = donor.Id }, ToDto(donor));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateDonor(int id, SaveDonorDto request)
    {
        var validationError = ValidateDonor(request);

        if (validationError is not null)
            return BadRequest(validationError);

        var donor = await _context.Donors.FindAsync(id);

        if (donor is null)
            return NotFound();

        donor.FirstName = request.FirstName;
        donor.LastName = request.LastName;
        donor.OrganizationName = request.OrganizationName;
        donor.Email = request.Email;
        donor.Phone = request.Phone;
        donor.Address = request.Address;
        donor.City = request.City;
        donor.Notes = request.Notes;
        donor.CreatedAt = request.CreatedAt;
        donor.DonorTypeId = request.DonorTypeId;
        donor.DonorStatusId = request.DonorStatusId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static string? ValidateDonor(SaveDonorDto request)
    {
        var hasIndividualName = !string.IsNullOrWhiteSpace(request.FirstName)
            && !string.IsNullOrWhiteSpace(request.LastName);
        var hasOrganizationName = !string.IsNullOrWhiteSpace(request.OrganizationName);

        if (!hasIndividualName && !hasOrganizationName)
            return "Upiši ime i prezime ili naziv organizacije.";

        if (request.DonorTypeId <= 0)
            return "Tip donatora je obavezan.";

        if (request.DonorStatusId <= 0)
            return "Status donatora je obavezan.";

        return null;
    }

    private static string GetDisplayName(Donor donor)
    {
        return !string.IsNullOrWhiteSpace(donor.OrganizationName)
            ? donor.OrganizationName
            : $"{donor.FirstName} {donor.LastName}".Trim();
    }

    private static DonorDto ToDto(Donor donor)
    {
        return new DonorDto
        {
            Id = donor.Id,
            FirstName = donor.FirstName,
            LastName = donor.LastName,
            OrganizationName = donor.OrganizationName,
            DisplayName = GetDisplayName(donor),
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
