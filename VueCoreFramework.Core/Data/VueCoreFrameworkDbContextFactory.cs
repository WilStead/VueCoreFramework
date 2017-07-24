using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace VueCoreFramework.Core.Data
{
    /// <summary>
    /// The <see cref="IDbContextFactory{TContext}"/> for <see cref="VueCoreFramework"/>.
    /// </summary>
    public class VueCoreFrameworkDbContextFactory : IDbContextFactory<VueCoreFrameworkDbContext>
    {
        /// <summary>
        /// Creates a new instance of <see cref="VueCoreFrameworkDbContext"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="VueCoreFrameworkDbContext"/>.</returns>
        public VueCoreFrameworkDbContext Create(DbContextFactoryOptions options)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(options.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{options.EnvironmentName}.json", optional: true);
            configurationBuilder.AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            var contextBuilder = new DbContextOptionsBuilder<VueCoreFrameworkDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(VueCoreFrameworkDbContext).GetTypeInfo().Assembly.GetName().Name;
            contextBuilder.UseSqlServer(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
            return new VueCoreFrameworkDbContext(contextBuilder.Options);
        }
    }
}
