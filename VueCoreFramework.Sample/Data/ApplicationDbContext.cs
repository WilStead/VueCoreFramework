using Microsoft.EntityFrameworkCore;
using VueCoreFramework.Core.Data;
using VueCoreFramework.Sample.Models;

namespace VueCoreFramework.Sample.Data
{
    /// <summary>
    /// The application's Entity Framework database context.
    /// </summary>
    public class ApplicationDbContext : VueCoreFrameworkDbContext
    {
        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of <see cref="Airline"/>s.
        /// </summary>
        public DbSet<Airline> Airlines { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of <see cref="City"/>s.
        /// </summary>
        public DbSet<City> Cities { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of <see cref="Country"/>s.
        /// </summary>
        public DbSet<Country> Countries { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of <see cref="Leader"/>s.
        /// </summary>
        public DbSet<Leader> Leaders { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        /// <summary>
        /// Configures the schema required for the framework.
        /// </summary>
        /// <param name="builder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Add your customizations after calling base.OnModelCreating(builder);

            // Sample data types
            builder.ApplyConfiguration(new AirlineCountryConfiguration());
        }
    }
}
