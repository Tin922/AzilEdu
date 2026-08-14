using System.Security.Claims;
using System.Text.Json;
using AzilEdu.Api.Data;
using AzilEdu.Api.Security;
using AzilEdu.Api.Services;
using AzilEdu.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private const int MaxInputLength = 4_000;

    private static readonly HashSet<string> AllowedTextPurposes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "animal-adoption",
            "donor-thank-you",
            "social-post"
        };

    private readonly IAiService _aiService;
    private readonly AzilEduDbContext _context;
    private readonly AiAuditService _auditService;

    public AiController(
        IAiService aiService,
        AzilEduDbContext context,
        AiAuditService auditService)
    {
        _aiService = aiService;
        _context = context;
        _auditService = auditService;
    }

    [Authorize(Policy = AuthorizationPolicies.Staff)]
    [HttpGet("status")]
    [DisableRateLimiting]
    public ActionResult<AiProviderStatusDto> GetStatus()
    {
        return Ok(new AiProviderStatusDto
        {
            Provider = _aiService.ProviderName,
            Model = _aiService.ModelName,
            UsesExternalService = _aiService.UsesExternalService
        });
    }

    [Authorize(Policy = AuthorizationPolicies.Staff)]
    [EnableRateLimiting("AiPerUser")]
    [HttpPost("text")]
    public async Task<ActionResult<AiTextResponseDto>> GenerateText(
        AiTextRequestDto request)
    {
        if (!AllowedTextPurposes.Contains(request.Purpose))
            return BadRequest("Nepodržana svrha AI zahtjeva.");

        if (string.IsNullOrWhiteSpace(request.Input))
            return BadRequest("Ulazni tekst je obavezan.");

        if (request.Input.Length > MaxInputLength)
            return BadRequest("Ulazni tekst je predugačak.");

        _auditService.LogRequest(User, request.Purpose);

        var text = await _aiService.GenerateTextAsync(
            request.Purpose,
            request.Input.Trim());

        return Ok(new AiTextResponseDto { Text = text });
    }

    [Authorize(Policy = AuthorizationPolicies.Staff)]
    [EnableRateLimiting("AiPerUser")]
    [HttpGet("daily-summary")]
    public async Task<ActionResult<AiTextResponseDto>> GetDailySummary()
    {
        var today = DateTime.Today;
        var lastSevenDays = today.AddDays(-7);

        var animalsCount = await _context.Animals.CountAsync();
        var animalsForAdoption = await _context.Animals
            .CountAsync(animal => animal.AnimalStatusId == 1);
        var openTasks = await _context.VolunteerTasks
            .CountAsync(task => task.VolunteerTaskStatusId != 4 &&
                                task.VolunteerTaskStatusId != 5);
        var overdueTasks = await _context.VolunteerTasks
            .CountAsync(task => task.DueDate.HasValue &&
                                task.DueDate.Value.Date < today &&
                                task.VolunteerTaskStatusId != 4 &&
                                task.VolunteerTaskStatusId != 5);
        var recentDonations = await _context.Donations
            .CountAsync(donation => donation.DonationDate >= lastSevenDays);

        var input =
            $"Ukupno životinja: {animalsCount}; " +
            $"dostupne za udomljenje: {animalsForAdoption}; " +
            $"otvoreni zadaci: {openTasks}; " +
            $"zakašnjeli zadaci: {overdueTasks}; " +
            $"donacije u zadnjih 7 dana: {recentDonations}.";

        _auditService.LogRequest(User, "daily-summary");

        var text = await _aiService.GenerateTextAsync("daily-summary", input);
        return Ok(new AiTextResponseDto { Text = text });
    }

    [Authorize(Roles = "Volunteer")]
    [EnableRateLimiting("AiPerUser")]
    [HttpGet("volunteer-summary/mine")]
    public async Task<ActionResult<AiTextResponseDto>> GetMyVolunteerSummary()
    {
        var volunteerClaim = User.FindFirst(AppClaimTypes.VolunteerId)?.Value;

        if (!int.TryParse(volunteerClaim, out var volunteerId))
            return Forbid();

        var tasks = await _context.VolunteerTasks
            .Include(task => task.Animal)
            .Include(task => task.VolunteerTaskType)
            .Include(task => task.VolunteerTaskStatus)
            .Where(task => task.VolunteerId == volunteerId &&
                           task.VolunteerTaskStatusId != 4 &&
                           task.VolunteerTaskStatusId != 5)
            .OrderBy(task => task.DueDate)
            .Take(10)
            .Select(task => new
            {
                task.Title,
                Type = task.VolunteerTaskType != null
                    ? task.VolunteerTaskType.Name
                    : string.Empty,
                Animal = task.Animal != null ? task.Animal.Name : string.Empty,
                Status = task.VolunteerTaskStatus != null
                    ? task.VolunteerTaskStatus.Name
                    : string.Empty,
                task.DueDate
            })
            .ToListAsync();

        if (tasks.Count == 0)
            return Ok(new AiTextResponseDto { Text = "Nema otvorenih zadataka." });

        var input = JsonSerializer.Serialize(tasks);

        _auditService.LogRequest(User, "volunteer-summary");

        var text = await _aiService.GenerateTextAsync("volunteer-summary", input);
        return Ok(new AiTextResponseDto { Text = text });
    }

    [Authorize(Policy = AuthorizationPolicies.Staff)]
    [EnableRateLimiting("AiPerUser")]
    [HttpPost("animal-intake")]
    public async Task<ActionResult<AnimalIntakeSuggestionDto>> SuggestAnimalIntake(
        AiFreeTextRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest("Unesi bilješku o životinji.");

        if (request.Text.Length > MaxInputLength)
            return BadRequest("Bilješka je predugačka.");

        _auditService.LogRequest(User, "animal-intake");

        var result = await _aiService
            .GenerateStructuredAsync<AnimalIntakeSuggestionDto>(
                "animal-intake",
                request.Text.Trim());

        if (!AiStructuredResponseValidator.TryValidateAnimalIntake(result, out var error))
            return BadRequest(error);

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.Staff)]
    [EnableRateLimiting("AiPerUser")]
    [HttpPost("animal-data-check")]
    public async Task<ActionResult<AnimalDataCheckDto>> CheckAnimalData(
        SaveAnimalDto request)
    {
        var safeInput = JsonSerializer.Serialize(new
        {
            request.Name,
            request.Species,
            request.Breed,
            request.Gender,
            request.Age,
            request.ArrivalDate,
            request.AnimalStatusId,
            request.Description
        });

        if (safeInput.Length > MaxInputLength)
            return BadRequest("Podaci za provjeru su predugački.");

        _auditService.LogRequest(User, "animal-data-check");

        var result = await _aiService
            .GenerateStructuredAsync<AnimalDataCheckDto>(
                "animal-data-check",
                safeInput);

        if (!AiStructuredResponseValidator.TryValidateAnimalDataCheck(result, out var error))
            return BadRequest(error);

        return Ok(result);
    }
}
