using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using VueCoreFramework.Core.Models;

namespace VueCoreFramework.Core.Data
{
    /// <summary>
    /// VueCoreFramework's core Entity Framework database context.
    /// </summary>
    public class VueCoreFrameworkDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of <see cref="Log"/>s.
        /// </summary>
        public DbSet<Log> Logs { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of <see cref="Message"/>s.
        /// </summary>
        public DbSet<Message> Messages { get; set; }

        /// <summary>
        /// Caches instances of <see cref="IRepository"/> for the entity types tracked by this
        /// <see cref="VueCoreFrameworkDbContext"/>.
        /// </summary>
        public IDictionary<string, IRepository> RepositoryCache { get; set; } = new Dictionary<string, IRepository>();

        /// <summary>
        /// Initializes a new instance of <see cref="VueCoreFrameworkDbContext"/>.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public VueCoreFrameworkDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets a <see cref="Repository{T}"/> for the given type, using a cached reference, if one
        /// already exists.
        /// </summary>
        /// <param name="type">The entity type of the repository.</param>
        public IRepository GetRepositoryForType(Type type)
        {
            if (!RepositoryCache.ContainsKey(type.FullName))
            {
                RepositoryCache.Add(type.FullName,
                    (IRepository)Activator.CreateInstance(typeof(Repository<>).MakeGenericType(type), this));
            }
            return RepositoryCache[type.FullName];
        }

        /// <summary>
        /// Configures the schema required for the framework.
        /// </summary>
        /// <param name="builder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<Message>()
                .Property(m => m.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();
        }

        /// <summary>
        /// Attempts to get a <see cref="Repository{T}"/> instance for the given data type.
        /// </summary>
        /// <param name="dataType">The data type requested.</param>
        /// <param name="repository">If successful, the <see cref="Repository{T}"/> instance.</param>
        /// <returns>true if a <see cref="Repository{T}"/> instance could be obtained; otherwise false.</returns>
        public bool TryGetRepository(string dataType, out IRepository repository)
        {
            repository = null;
            if (string.IsNullOrEmpty(dataType))
            {
                return false;
            }
            var entity = Model.GetEntityTypes().FirstOrDefault(e => e.Name.Substring(e.Name.LastIndexOf('.') + 1) == dataType);
            if (entity == null)
            {
                return false;
            }
            var type = entity.ClrType;
            if (type == null)
            {
                return false;
            }
            repository = GetRepositoryForType(type);
            return true;
        }
    }
}