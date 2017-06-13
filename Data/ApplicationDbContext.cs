using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVCCoreVue.Models;

namespace MVCCoreVue.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Log> Logs { get; set; }
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Leader> Leaders { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Country>()
                .HasOne(c => c.Leader)
                .WithOne(c => c.Country)
                .HasForeignKey<Leader>(c => c.LeaderCountryId)
                .OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade);

            builder.Entity<AirlineCountry>()
                .HasKey(c => new { c.CountryId, c.AirlineId });
            builder.Entity<AirlineCountry>()
                .HasOne(c => c.Airline)
                .WithMany(c => c.AirlineCountries)
                .HasForeignKey(c => c.AirlineId);
            builder.Entity<AirlineCountry>()
                .HasOne(c => c.Country)
                .WithMany(c => c.CountryAirlines)
                .HasForeignKey(c => c.CountryId);
        }
    }
}