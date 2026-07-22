using AzilEdu.Api.Data;
using AzilEdu.Shared.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AzilEduDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AzilEduDbContext>();

    await db.Database.MigrateAsync();

    if (!await db.Animals.AnyAsync())
    {
        db.Animals.AddRange(
            new Animal
            {
                Name = "Luna",
                Species = "Pas",
                Breed = "Labrador",
                Gender = "Ženka",
                Age = 3,
                ArrivalDate = new DateTime(2025, 10, 12),
                AnimalStatusId = 1,
                ImageUrl = "/images/animals/luna.webp",
                Description = "Mirna i druželjubiva kujica koja voli šetnje."
            },
            new Animal
            {
                Name = "Maza",
                Species = "Mačka",
                Breed = "Domaća kratkodlaka",
                Gender = "Ženka",
                Age = 2,
                ArrivalDate = new DateTime(2025, 11, 5),
                AnimalStatusId = 3,
                ImageUrl = "/images/animals/maza.webp",
                Description = "Zaigrana mačka naviknuta na boravak u zatvorenom prostoru."
            },
            new Animal
            {
                Name = "Rex",
                Species = "Pas",
                Breed = "Njemački ovčar",
                Gender = "Mužjak",
                Age = 5,
                ArrivalDate = new DateTime(2026, 1, 20),
                AnimalStatusId = 1,
                ImageUrl = "/images/animals/rex.webp",
                Description = "Aktivan pas koji traži iskusnijeg vlasnika."
            },
            new Animal
            {
                Name = "Nala",
                Species = "Mačka",
                Breed = "Maine Coon mješanac",
                Gender = "Ženka",
                Age = null,
                ArrivalDate = new DateTime(2026, 2, 3),
                AnimalStatusId = 2,
                ImageUrl = "/images/animals/nala.webp",
                Description = "Mlada mačka pronađena bez poznate povijesti."
            },
            new Animal
            {
                Name = "Tobi",
                Species = "Pas",
                Breed = "Mješanac",
                Gender = "Mužjak",
                Age = 1,
                ArrivalDate = null,
                AnimalStatusId = 4,
                ImageUrl = "/images/animals/tobi.webp",
                Description = "Vesel pas kojem datum dolaska još nije potvrđen."
            },
            new Animal
            {
                Name = "Bruno",
                Species = "Pas",
                Breed = "Bigl",
                Gender = "Mužjak",
                Age = 4,
                ArrivalDate = new DateTime(2025, 9, 18),
                AnimalStatusId = 3,
                ImageUrl = "/images/animals/bruno.webp",
                Description = "Udomljen pas koji ostaje u evidenciji azila."
            }
        );
    }

    if (!await db.Volunteers.AnyAsync())
    {
        db.Volunteers.AddRange(
            new Volunteer
            {
                FirstName = "Ana",
                LastName = "Horvat",
                Email = "ana.horvat@example.com",
                Phone = "091 111 2222",
                Skills = "Šetnja pasa, socijalizacija",
                AvailableFrom = new DateTime(2026, 7, 1),
                Notes = "Dostupna vikendom.",
                VolunteerStatusId = 2
            },
            new Volunteer
            {
                FirstName = "Marko",
                LastName = "Kovač",
                Email = "marko.kovac@example.com",
                Phone = "092 333 4444",
                Skills = "Prijevoz, pomoć kod veterinara",
                AvailableFrom = new DateTime(2026, 7, 10),
                Notes = "Ima vlastiti automobil.",
                VolunteerStatusId = 1
            }
        );
    }

    if (!await db.Donors.AnyAsync())
    {
        db.Donors.AddRange(
            new Donor
            {
                FirstName = "Ivana",
                LastName = "Babić",
                Email = "ivana.babic@example.com",
                Phone = "095 555 1212",
                Address = "Ulica donatora 1",
                City = "Osijek",
                Notes = "Donira hranu jednom mjesečno.",
                CreatedAt = new DateTime(2026, 6, 15),
                DonorTypeId = 1,
                DonorStatusId = 2
            },
            new Donor
            {
                OrganizationName = "Pet Plus d.o.o.",
                Email = "kontakt@petplus.example.com",
                Phone = "031 555 000",
                Address = "Industrijska 12",
                City = "Osijek",
                Notes = "Potencijalni donator opreme.",
                CreatedAt = new DateTime(2026, 6, 20),
                DonorTypeId = 2,
                DonorStatusId = 1
            }
        );
    }

    if (!await db.Employees.AnyAsync())
    {
        db.Employees.AddRange(
            new Employee
            {
                FirstName = "Petra",
                LastName = "Novak",
                Email = "petra.novak@aziledu.example.com",
                Phone = "031 100 200",
                EmployeeNumber = "EMP-001",
                HireDate = new DateTime(2025, 3, 1),
                Notes = "Koordinira raspored volontera.",
                EmployeePositionId = 3,
                EmployeeStatusId = 1
            },
            new Employee
            {
                FirstName = "Ivan",
                LastName = "Marić",
                Email = "ivan.maric@aziledu.example.com",
                Phone = "031 100 201",
                EmployeeNumber = "EMP-002",
                HireDate = new DateTime(2024, 9, 10),
                Notes = "Zadužen za svakodnevnu brigu o životinjama.",
                EmployeePositionId = 1,
                EmployeeStatusId = 1
            }
        );
    }

    if (!await db.VolunteerTasks.AnyAsync())
    {
        db.VolunteerTasks.AddRange(
            new VolunteerTask
            {
                Title = "Šetnja s Lunom",
                Description = "Jutarnja šetnja u parku.",
                DueDate = DateTime.Today.AddDays(2),
                VolunteerId = 1,
                AnimalId = 1,
                VolunteerTaskStatusId = 2,
                VolunteerTaskTypeId = 1,
                Notes = "Ponijeti povodac."
            },
            new VolunteerTask
            {
                Title = "Čišćenje boksa",
                Description = "Generalno čišćenje boksa A1.",
                DueDate = DateTime.Today.AddDays(-3),
                VolunteerId = 2,
                AnimalId = 3,
                VolunteerTaskStatusId = 1,
                VolunteerTaskTypeId = 3,
                Notes = "Rok je prošao — hitno."
            },
            new VolunteerTask
            {
                Title = "Socijalizacija Maze",
                Description = "Igra i druženje s mačkom.",
                DueDate = DateTime.Today.AddDays(-1),
                AnimalId = 2,
                VolunteerTaskStatusId = 3,
                VolunteerTaskTypeId = 4,
                Notes = "Volonter još nije dodijeljen."
            }
        );
    }

    if (!await db.Donations.AnyAsync())
    {
        db.Donations.AddRange(
            new Donation
            {
                DonorId = 1,
                DonationTypeId = 1,
                DonationStatusId = 2,
                DonationDate = new DateTime(2026, 6, 20),
                Amount = 150.00m,
                Notes = "Mjesečna novčana donacija."
            },
            new Donation
            {
                DonorId = 2,
                DonationTypeId = 2,
                DonationStatusId = 1,
                DonationDate = new DateTime(2026, 7, 10),
                ItemName = "Suha hrana za pse",
                Quantity = 10,
                EstimatedValue = 80.00m,
                Notes = "Donacija hrane — čeka potvrdu."
            },
            new Donation
            {
                DonorId = 1,
                DonationTypeId = 3,
                DonationStatusId = 3,
                DonationDate = new DateTime(2026, 5, 15),
                ItemName = "Transporter za mačke",
                Quantity = 2,
                EstimatedValue = 120.00m,
                Notes = "Oprema iskorištena."
            }
        );
    }

    await db.SaveChangesAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();

app.Run();
