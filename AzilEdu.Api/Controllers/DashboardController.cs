using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private const int CompletedTaskStatusId = 4;
    private const int MoneyDonationTypeId = 1;
    private const int PendingDonationStatusId = 1;

    private readonly AzilEduDbContext _context;

    public DashboardController(AzilEduDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        // Kasnije će admin vidjeti sve podatke, a ostale role samo svoj dio aplikacije.
        var today = DateTime.Today;

        var summary = new DashboardSummaryDto
        {
            AnimalsCount = await _context.Animals.CountAsync(),
            AvailableAnimalsCount = await _context.Animals.CountAsync(animal => animal.AnimalStatusId == 1),
            ActiveVolunteersCount = await _context.Volunteers.CountAsync(volunteer => volunteer.VolunteerStatusId == 2),
            OpenVolunteerTasksCount = await _context.VolunteerTasks.CountAsync(task => task.VolunteerTaskStatusId == 1),
            ActiveDonorsCount = await _context.Donors.CountAsync(donor => donor.DonorStatusId == 2),
            EmployeesCount = await _context.Employees.CountAsync(),
            DonationsCount = await _context.Donations.CountAsync(),
            PendingDonationsCount = await _context.Donations.CountAsync(d => d.DonationStatusId == PendingDonationStatusId),
            MoneyDonationsTotal = await _context.Donations
                .Where(d => d.DonationTypeId == MoneyDonationTypeId && d.Amount.HasValue)
                .SumAsync(d => d.Amount ?? 0),
            EstimatedMaterialDonationsTotal = await _context.Donations
                .Where(d => d.DonationTypeId != MoneyDonationTypeId && d.EstimatedValue.HasValue)
                .SumAsync(d => d.EstimatedValue ?? 0),
            OverdueVolunteerTasksCount = await _context.VolunteerTasks.CountAsync(task =>
                task.DueDate.HasValue
                && task.DueDate.Value.Date < today
                && task.VolunteerTaskStatusId != CompletedTaskStatusId)
        };

        return Ok(summary);
    }

    [HttpGet("recent-donations")]
    public async Task<ActionResult<List<RecentDonationDto>>> GetRecentDonations()
    {
        var donations = await _context.Donations
            .Include(d => d.Donor)
            .Include(d => d.DonationType)
            .OrderByDescending(d => d.DonationDate)
            .ThenByDescending(d => d.Id)
            .Take(5)
            .Select(d => new RecentDonationDto
            {
                Id = d.Id,
                DonorName = d.Donor != null
                    ? (d.Donor.OrganizationName != ""
                        ? d.Donor.OrganizationName
                        : d.Donor.FirstName + " " + d.Donor.LastName)
                    : string.Empty,
                DonationType = d.DonationType != null ? d.DonationType.Name : string.Empty,
                DonationDate = d.DonationDate,
                Amount = d.Amount,
                ItemName = d.ItemName
            })
            .ToListAsync();

        return Ok(donations);
    }
}
