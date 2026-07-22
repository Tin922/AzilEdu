using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using AzilEdu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationsController : ControllerBase
{
    private const int MoneyDonationTypeId = 1;
    private const int PendingDonationStatusId = 1;

    private readonly AzilEduDbContext _context;

    public DonationsController(AzilEduDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<DonationDto>>> GetDonations(
        [FromQuery] int? donorId,
        [FromQuery] int? typeId,
        [FromQuery] int? statusId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        // Kasnije će donator vidjeti samo svoje donacije.
        var query = _context.Donations
            .Include(donation => donation.Donor)
            .Include(donation => donation.DonationType)
            .Include(donation => donation.DonationStatus)
            .AsQueryable();

        if (donorId.HasValue)
            query = query.Where(donation => donation.DonorId == donorId.Value);

        if (typeId.HasValue)
            query = query.Where(donation => donation.DonationTypeId == typeId.Value);

        if (statusId.HasValue)
            query = query.Where(donation => donation.DonationStatusId == statusId.Value);

        if (dateFrom.HasValue)
            query = query.Where(donation => donation.DonationDate >= dateFrom.Value.Date);

        if (dateTo.HasValue)
            query = query.Where(donation => donation.DonationDate <= dateTo.Value.Date);

        var donations = await query
            .OrderByDescending(donation => donation.DonationDate)
            .ThenByDescending(donation => donation.Id)
            .ToListAsync();

        return Ok(donations.Select(ToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DonationDto>> GetDonationById(int id)
    {
        var donation = await _context.Donations
            .Include(d => d.Donor)
            .Include(d => d.DonationType)
            .Include(d => d.DonationStatus)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (donation is null)
            return NotFound();

        return Ok(ToDto(donation));
    }

    [HttpPost]
    public async Task<ActionResult<DonationDto>> CreateDonation(SaveDonationDto request)
    {
        var validationError = ValidateDonation(request);
        if (validationError is not null)
            return BadRequest(validationError);

        var donation = MapToEntity(request);
        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();

        await _context.Entry(donation).Reference(d => d.Donor).LoadAsync();
        await _context.Entry(donation).Reference(d => d.DonationType).LoadAsync();
        await _context.Entry(donation).Reference(d => d.DonationStatus).LoadAsync();

        return CreatedAtAction(nameof(GetDonationById), new { id = donation.Id }, ToDto(donation));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDonation(int id, SaveDonationDto request)
    {
        var donation = await _context.Donations.FindAsync(id);

        if (donation is null)
            return NotFound();

        var validationError = ValidateDonation(request);
        if (validationError is not null)
            return BadRequest(validationError);

        donation.DonorId = request.DonorId;
        donation.DonationTypeId = request.DonationTypeId;
        donation.DonationStatusId = request.DonationStatusId;
        donation.DonationDate = request.DonationDate.Date;
        donation.Amount = request.DonationTypeId == MoneyDonationTypeId ? request.Amount : null;
        donation.ItemName = request.DonationTypeId == MoneyDonationTypeId ? string.Empty : request.ItemName.Trim();
        donation.Quantity = request.DonationTypeId == MoneyDonationTypeId ? null : request.Quantity;
        donation.EstimatedValue = request.DonationTypeId == MoneyDonationTypeId ? null : request.EstimatedValue;
        donation.Notes = request.Notes.Trim();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDonation(int id)
    {
        var donation = await _context.Donations.FindAsync(id);

        if (donation is null)
            return NotFound();

        _context.Donations.Remove(donation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static Donation MapToEntity(SaveDonationDto request)
    {
        return new Donation
        {
            DonorId = request.DonorId,
            DonationTypeId = request.DonationTypeId,
            DonationStatusId = request.DonationStatusId,
            DonationDate = request.DonationDate.Date,
            Amount = request.DonationTypeId == MoneyDonationTypeId ? request.Amount : null,
            ItemName = request.DonationTypeId == MoneyDonationTypeId ? string.Empty : request.ItemName.Trim(),
            Quantity = request.DonationTypeId == MoneyDonationTypeId ? null : request.Quantity,
            EstimatedValue = request.DonationTypeId == MoneyDonationTypeId ? null : request.EstimatedValue,
            Notes = request.Notes.Trim()
        };
    }

    private static string? ValidateDonation(SaveDonationDto request)
    {
        if (request.DonorId == 0)
            return "Donator je obavezan.";

        if (request.DonationTypeId == 0)
            return "Tip donacije je obavezan.";

        if (request.DonationStatusId == 0)
            return "Status donacije je obavezan.";

        if (request.DonationTypeId == MoneyDonationTypeId && (!request.Amount.HasValue || request.Amount <= 0))
            return "Novčana donacija mora imati iznos veći od nule.";

        if (request.DonationTypeId != MoneyDonationTypeId && string.IsNullOrWhiteSpace(request.ItemName))
            return "Materijalna donacija mora imati naziv stvari.";

        return null;
    }

    internal static DonationDto ToDto(Donation donation)
    {
        var donorName = donation.Donor != null
            ? (donation.Donor.OrganizationName != ""
                ? donation.Donor.OrganizationName
                : donation.Donor.FirstName + " " + donation.Donor.LastName)
            : string.Empty;

        var typeName = donation.DonationType?.Name ?? string.Empty;
        var displayValue = donation.DonationTypeId == MoneyDonationTypeId
            ? $"{donation.Amount:N2} EUR"
            : $"{donation.ItemName} ({donation.Quantity})";

        return new DonationDto
        {
            Id = donation.Id,
            DonorId = donation.DonorId,
            DonorName = donorName,
            DonationTypeId = donation.DonationTypeId,
            DonationType = typeName,
            DonationStatusId = donation.DonationStatusId,
            DonationStatus = donation.DonationStatus?.Name ?? string.Empty,
            DonationDate = donation.DonationDate,
            Amount = donation.Amount,
            ItemName = donation.ItemName,
            Quantity = donation.Quantity,
            EstimatedValue = donation.EstimatedValue,
            Notes = donation.Notes,
            DisplayValue = displayValue
        };
    }
}
