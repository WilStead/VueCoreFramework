using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVCCoreVue.Models;

namespace MVCCoreVue.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Log> Logs { get; set; }

        public DbSet<Airline> Airlines { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Country> Countries { get; set; }
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

            builder.Entity<AirlineCountry>()
                .HasKey(c => new { c.CountriesId, c.AirlinesId });
            builder.Entity<AirlineCountry>()
                .HasOne(c => c.Airlines)
                .WithMany(c => c.Countries)
                .HasForeignKey(c => c.AirlinesId);
            builder.Entity<AirlineCountry>()
                .HasOne(c => c.Countries)
                .WithMany(c => c.Airlines)
                .HasForeignKey(c => c.CountriesId);
        }
    }
}