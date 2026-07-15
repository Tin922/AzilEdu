using AzilEdu.Api.Data;
using AzilEdu.Shared.DTOs;
using AzilEdu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AzilEduDbContext _context;
    public EmployeesController(AzilEduDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll() =>
        Ok(await _context.Employees.Include(e => e.EmployeePosition).Include(e => e.EmployeeStatus).OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .Select(e => new EmployeeDto
            {
                Id = e.Id, FirstName = e.FirstName, LastName = e.LastName, Email = e.Email, Phone = e.Phone,
                EmployeeNumber = e.EmployeeNumber, HireDate = e.HireDate, Notes = e.Notes,
                EmployeePositionId = e.EmployeePositionId, Position = e.EmployeePosition!.Name,
                EmployeeStatusId = e.EmployeeStatusId, Status = e.EmployeeStatus!.Name
            }).ToListAsync());

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(SaveEmployeeDto dto)
    {
        var e = new Employee
        {
            FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email, Phone = dto.Phone,
            EmployeeNumber = dto.EmployeeNumber, HireDate = dto.HireDate, Notes = dto.Notes,
            EmployeePositionId = dto.EmployeePositionId, EmployeeStatusId = dto.EmployeeStatusId
        };
        _context.Employees.Add(e);
        await _context.SaveChangesAsync();
        return Ok(await _context.Employees.Include(x => x.EmployeePosition).Include(x => x.EmployeeStatus).Where(x => x.Id == e.Id)
            .Select(x => new EmployeeDto
            {
                Id = x.Id, FirstName = x.FirstName, LastName = x.LastName, Email = x.Email, Phone = x.Phone,
                EmployeeNumber = x.EmployeeNumber, HireDate = x.HireDate, Notes = x.Notes,
                EmployeePositionId = x.EmployeePositionId, Position = x.EmployeePosition!.Name,
                EmployeeStatusId = x.EmployeeStatusId, Status = x.EmployeeStatus!.Name
            }).FirstAsync());
    }
}
