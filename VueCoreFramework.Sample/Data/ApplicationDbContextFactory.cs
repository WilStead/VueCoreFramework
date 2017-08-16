using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;

namespace VueCoreFramework.Sample.Data
{
    /// <summary>
    /// The <see cref="IDesignTimeDbContextFactory{TContext}"/> for <see cref="ApplicationDbContext"/>.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="ApplicationDbContext"/>.</returns>
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);
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
