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
    public DonorsController(AzilEduDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<List<DonorDto>>> GetAll() =>
        Ok(await _context.Donors.Include(d => d.DonorType).Include(d => d.DonorStatus).OrderByDescending(d => d.CreatedAt)
            .Select(d => new DonorDto
            {
                Id = d.Id, FirstName = d.FirstName, LastName = d.LastName, OrganizationName = d.OrganizationName,
                Email = d.Email, Phone = d.Phone, Address = d.Address, City = d.City, Notes = d.Notes, CreatedAt = d.CreatedAt,
                DonorTypeId = d.DonorTypeId, Type = d.DonorType!.Name, DonorStatusId = d.DonorStatusId, Status = d.DonorStatus!.Name
            }).ToListAsync());

    [HttpPost]
    public async Task<ActionResult<DonorDto>> Create(SaveDonorDto dto)
    {
        var d = new Donor
        {
            FirstName = dto.FirstName, LastName = dto.LastName, OrganizationName = dto.OrganizationName,
            Email = dto.Email, Phone = dto.Phone, Address = dto.Address, City = dto.City, Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow, DonorTypeId = dto.DonorTypeId, DonorStatusId = dto.DonorStatusId
        };
        _context.Donors.Add(d);
        await _context.SaveChangesAsync();
        return Ok(await _context.Donors.Include(x => x.DonorType).Include(x => x.DonorStatus).Where(x => x.Id == d.Id)
            .Select(x => new DonorDto
            {
                Id = x.Id, FirstName = x.FirstName, LastName = x.LastName, OrganizationName = x.OrganizationName,
                Email = x.Email, Phone = x.Phone, Address = x.Address, City = x.City, Notes = x.Notes, CreatedAt = x.CreatedAt,
                DonorTypeId = x.DonorTypeId, Type = x.DonorType!.Name, DonorStatusId = x.DonorStatusId, Status = x.DonorStatus!.Name
            }).FirstAsync());
    }
}
