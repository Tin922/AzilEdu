using AzilEdu.Api.Data;
using Microsoft.EntityFrameworkCore;
using AzilEdu.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AzilEduDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

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
                IsAdopted = false,
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
                IsAdopted = true,
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
                IsAdopted = false,
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
                IsAdopted = false,
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
                IsAdopted = false,
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
                IsAdopted = true,
                ImageUrl = "/images/animals/bruno.webp",
                Description = "Udomljen pas koji ostaje u evidenciji azila."
            }
        );
 
        await db.SaveChangesAsync();
    }
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AzilEduDbContext>();
 
    await db.Database.MigrateAsync();
 
    if (!await db.HousingUnits.AnyAsync())
    {
        db.HousingUnits.AddRange(
            new HousingUnit
            {
                Id = 1,
                Name = "Boks A1",
                UnitType = "Boks",
                Capacity = 2,
                Occupied = 2,
                LastCleanedAt = new DateTime(2026, 6, 10),
                IsActive = true,
                ImageUrl = "/images/units/boks-a1.webp",
                Note = "Rezervirano za velike pasmine."
            },
            new HousingUnit
            {
                Id = 2,
                Name = "Boks A2",
                UnitType = "Boks",
                Capacity = 2,
                Occupied = 1,
                LastCleanedAt = new DateTime(2026, 6, 14),
                IsActive = true,
                ImageUrl = "/images/units/boks-a2.webp",
                Note = ""
            },
            new HousingUnit
            {
                Id = 3,
                Name = "Soba M1",
                UnitType = "Soba",
                Capacity = 4,
                Occupied = 2,
                LastCleanedAt = null,
                IsActive = true,
                ImageUrl = "/images/units/soba-m1.webp",
                Note = "Prostor za mačke, tiši dio azila."
            },
            new HousingUnit
            {
                Id = 4,
                Name = "Karantena K1",
                UnitType = "Karantena",
                Capacity = 3,
                Occupied = 1,
                LastCleanedAt = new DateTime(2026, 6, 15),
                IsActive = true,
                ImageUrl = "/images/units/karantena-k1.webp",
                Note = "Samo za novoprimljene životinje."
            },
            new HousingUnit
            {
                Id = 5,
                Name = "Boks B1",
                UnitType = "Boks",
                Capacity = 2,
                Occupied = 0,
                LastCleanedAt = new DateTime(2026, 5, 20),
                IsActive = false,
                ImageUrl = "/images/units/boks-b1.webp",
                Note = "Privremeno van upotrebe zbog renovacije."
            },
            new HousingUnit
            {
                Id = 6,
                Name = "Soba M2",
                UnitType = "Soba",
                Capacity = 5,
                Occupied = 3,
                LastCleanedAt = new DateTime(2026, 6, 12),
                IsActive = true,
                ImageUrl = "/images/units/soba-m2.webp",
                Note = ""
            }
        );
         await db.SaveChangesAsync(); 
    }
}

app.UseAuthorization();

app.MapControllers();

app.Run();
