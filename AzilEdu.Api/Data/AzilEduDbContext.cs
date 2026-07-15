using AzilEdu.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Data;

public class AzilEduDbContext : DbContext
{
    public AzilEduDbContext(DbContextOptions<AzilEduDbContext> options) : base(options) { }

    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<AnimalStatus> AnimalStatuses => Set<AnimalStatus>();
    public DbSet<Volunteer> Volunteers => Set<Volunteer>();
    public DbSet<VolunteerStatus> VolunteerStatuses => Set<VolunteerStatus>();
    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<DonorType> DonorTypes => Set<DonorType>();
    public DbSet<DonorStatus> DonorStatuses => Set<DonorStatus>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeePosition> EmployeePositions => Set<EmployeePosition>();
    public DbSet<EmployeeStatus> EmployeeStatuses => Set<EmployeeStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Animal>()
            .HasOne(a => a.AnimalStatus).WithMany(s => s.Animals)
            .HasForeignKey(a => a.AnimalStatusId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AnimalStatus>().HasData(
            new AnimalStatus { Id = 1, Name = "Dostupna za udomljenje" },
            new AnimalStatus { Id = 2, Name = "Rezervirana" },
            new AnimalStatus { Id = 3, Name = "Udomljena" },
            new AnimalStatus { Id = 4, Name = "Na liječenju" });

        modelBuilder.Entity<Volunteer>()
            .HasOne(v => v.VolunteerStatus).WithMany(s => s.Volunteers)
            .HasForeignKey(v => v.VolunteerStatusId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VolunteerStatus>().HasData(
            new VolunteerStatus { Id = 1, Name = "Novi" },
            new VolunteerStatus { Id = 2, Name = "Aktivan" },
            new VolunteerStatus { Id = 3, Name = "Privremeno nedostupan" },
            new VolunteerStatus { Id = 4, Name = "Neaktivan" });

        modelBuilder.Entity<Donor>()
            .HasOne(d => d.DonorType).WithMany(t => t.Donors)
            .HasForeignKey(d => d.DonorTypeId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Donor>()
            .HasOne(d => d.DonorStatus).WithMany(s => s.Donors)
            .HasForeignKey(d => d.DonorStatusId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DonorType>().HasData(
            new DonorType { Id = 1, Name = "Fizička osoba" },
            new DonorType { Id = 2, Name = "Tvrtka" },
            new DonorType { Id = 3, Name = "Udruga ili organizacija" });

        modelBuilder.Entity<DonorStatus>().HasData(
            new DonorStatus { Id = 1, Name = "Novi" },
            new DonorStatus { Id = 2, Name = "Aktivan" },
            new DonorStatus { Id = 3, Name = "Povremeni" },
            new DonorStatus { Id = 4, Name = "Neaktivan" });

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.EmployeePosition).WithMany(p => p.Employees)
            .HasForeignKey(e => e.EmployeePositionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.EmployeeStatus).WithMany(s => s.Employees)
            .HasForeignKey(e => e.EmployeeStatusId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmployeePosition>().HasData(
            new EmployeePosition { Id = 1, Name = "Djelatnik azila" },
            new EmployeePosition { Id = 2, Name = "Veterinar" },
            new EmployeePosition { Id = 3, Name = "Koordinator volontera" },
            new EmployeePosition { Id = 4, Name = "Administrator" });

        modelBuilder.Entity<EmployeeStatus>().HasData(
            new EmployeeStatus { Id = 1, Name = "Aktivan" },
            new EmployeeStatus { Id = 2, Name = "Na dopustu ili bolovanju" },
            new EmployeeStatus { Id = 3, Name = "Neaktivan" });
    }
}
