using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace VueCoreFramework.Sample.Data
{
    /// <summary>
    /// The <see cref="IDbContextFactory{TContext}"/> for <see cref="ApplicationDbContext"/>.
    /// </summary>
    public class ApplicationDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="ApplicationDbContext"/>.</returns>
        public ApplicationDbContext Create(DbContextFactoryOptions options)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(options.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{options.EnvironmentName}.json", optional: true);
            configurationBuilder.AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            var contextBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(ApplicationDbContext).GetTypeInfo().Assembly.GetName().Name;
            contextBuilder.UseSqlServer(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
            return new ApplicationDbContext(contextBuilder.Options);
        }
    }
}
