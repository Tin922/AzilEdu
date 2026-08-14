using AzilEdu.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Data;

/// <summary>
/// Idempotent demo podaci za testiranje svih uloga.
/// Dopunjuje postojeću bazu ako pojedinačnom korisniku nedostaju zadaci ili donacije.
/// </summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(AzilEduDbContext db)
    {
        var primaryVolunteerId = await db.Volunteers
            .OrderBy(volunteer => volunteer.Id)
            .Select(volunteer => volunteer.Id)
            .FirstOrDefaultAsync();

        var secondaryVolunteerId = await db.Volunteers
            .OrderBy(volunteer => volunteer.Id)
            .Select(volunteer => volunteer.Id)
            .Skip(1)
            .FirstOrDefaultAsync();

        var primaryDonorId = await db.Donors
            .OrderBy(donor => donor.Id)
            .Select(donor => donor.Id)
            .FirstOrDefaultAsync();

        var secondaryDonorId = await db.Donors
            .OrderBy(donor => donor.Id)
            .Select(donor => donor.Id)
            .Skip(1)
            .FirstOrDefaultAsync();

        if (primaryVolunteerId > 0)
        {
            await EnsureVolunteerTasksAsync(db, primaryVolunteerId, secondaryVolunteerId);
        }

        if (primaryDonorId > 0)
        {
            await EnsureDonationsAsync(db, primaryDonorId, secondaryDonorId);
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureVolunteerTasksAsync(
        AzilEduDbContext db,
        int primaryVolunteerId,
        int secondaryVolunteerId)
    {
        if (!await db.VolunteerTasks.AnyAsync(task => task.VolunteerId == primaryVolunteerId))
        {
            db.VolunteerTasks.AddRange(
                new VolunteerTask
                {
                    Title = "Šetnja s Lunom",
                    Description = "Redovna jutarnja šetnja i socijalizacija.",
                    DueDate = DateTime.Today.AddDays(2),
                    Notes = "Povodac je u ostavi.",
                    VolunteerId = primaryVolunteerId,
                    AnimalId = 1,
                    VolunteerTaskStatusId = 2,
                    VolunteerTaskTypeId = 1
                },
                new VolunteerTask
                {
                    Title = "Hranjenje mačaka",
                    Description = "Popodnevno hranjenje u katu B.",
                    DueDate = DateTime.Today.AddDays(-1),
                    Notes = "Provjeri količinu vode.",
                    VolunteerId = primaryVolunteerId,
                    AnimalId = 2,
                    VolunteerTaskStatusId = 3,
                    VolunteerTaskTypeId = 2
                });
        }

        if (secondaryVolunteerId > 0 &&
            !await db.VolunteerTasks.AnyAsync(task => task.VolunteerId == secondaryVolunteerId))
        {
            db.VolunteerTasks.Add(
                new VolunteerTask
                {
                    Title = "Pomoć oko transporta",
                    Description = "Prijevoz Rexa na kontrolu.",
                    DueDate = DateTime.Today.AddDays(5),
                    Notes = "Volonter vozi vlastiti automobil.",
                    VolunteerId = secondaryVolunteerId,
                    AnimalId = 3,
                    VolunteerTaskStatusId = 2,
                    VolunteerTaskTypeId = 5
                });
        }

        if (!await db.VolunteerTasks.AnyAsync(task => task.VolunteerTaskStatusId == 1))
        {
            db.VolunteerTasks.Add(
                new VolunteerTask
                {
                    Title = "Socijalizacija Nale",
                    Description = "Kratka igra i navikavanje na ljude.",
                    DueDate = DateTime.Today.AddDays(3),
                    Notes = "Još nije dodijeljen volonter — employee može dodijeliti.",
                    VolunteerId = null,
                    AnimalId = 4,
                    VolunteerTaskStatusId = 1,
                    VolunteerTaskTypeId = 4
                });
        }
    }

    private static async Task EnsureDonationsAsync(
        AzilEduDbContext db,
        int primaryDonorId,
        int secondaryDonorId)
    {
        if (!await db.Donations.AnyAsync(donation => donation.DonorId == primaryDonorId))
        {
            db.Donations.AddRange(
                new Donation
                {
                    DonorId = primaryDonorId,
                    DonationTypeId = 1,
                    DonationStatusId = 2,
                    DonationDate = DateTime.Today.AddDays(-14),
                    Amount = 150m,
                    Notes = "Mjesečna novčana podrška azilu."
                },
                new Donation
                {
                    DonorId = primaryDonorId,
                    DonationTypeId = 2,
                    DonationStatusId = 2,
                    DonationDate = DateTime.Today.AddDays(-3),
                    ItemName = "Suha hrana za pse",
                    Quantity = 2,
                    EstimatedValue = 80m,
                    Notes = "Vreće od 15 kg."
                });
        }

        if (secondaryDonorId > 0 &&
            !await db.Donations.AnyAsync(donation => donation.DonorId == secondaryDonorId))
        {
            db.Donations.Add(
                new Donation
                {
                    DonorId = secondaryDonorId,
                    DonationTypeId = 3,
                    DonationStatusId = 1,
                    DonationDate = DateTime.Today.AddDays(-7),
                    ItemName = "Transporteri za mačke",
                    Quantity = 3,
                    EstimatedValue = 120m,
                    Notes = "Pilot donacija opreme — status Evidentirana."
                });
        }
    }
}
