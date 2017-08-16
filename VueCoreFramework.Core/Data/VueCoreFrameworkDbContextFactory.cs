using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;

namespace VueCoreFramework.Core.Data
{
    /// <summary>
    /// The <see cref="IDesignTimeDbContextFactory{TContext}"/> for <see cref="VueCoreFramework"/>.
    /// </summary>
    public class VueCoreFrameworkDbContextFactory : IDesignTimeDbContextFactory<VueCoreFrameworkDbContext>
    {
        /// <summary>
        /// Creates a new instance of <see cref="VueCoreFrameworkDbContext"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="VueCoreFrameworkDbContext"/>.</returns>
        public VueCoreFrameworkDbContext CreateDbContext(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            var contextBuilder = new DbContextOptionsBuilder<VueCoreFrameworkDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(VueCoreFrameworkDbContext).GetTypeInfo().Assembly.GetName().Name;
            contextBuilder.UseSqlServer(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
            return new VueCoreFrameworkDbContext(contextBuilder.Options);
        }
    }
}
