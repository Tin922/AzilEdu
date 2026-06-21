using AzilEdu.Shared.Models;
using Microsoft.EntityFrameworkCore;
 
namespace AzilEdu.Api.Data;
 
public class AzilEduDbContext : DbContext
{
    public AzilEduDbContext(DbContextOptions<AzilEduDbContext> options)
        : base(options)
    {
    }
 
    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<HousingUnit> HousingUnits => Set<HousingUnit>();
    
    //Dodavanjem DbSet<HousingUnit> u AzilEduDbContext, EF Core je prepoznao novi entitet i kroz migraciju
    //generirao novu tablicu HousingUnits u SQLite bazi, sa stupcima koji odgovaraju svojstvima klase
}