using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using VueCoreFramework.Core.Configuration;

namespace VueCoreFramework.Auth
{
    /// <summary>
    /// The main class of the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        /// <param name="args">Any command-line arguments passed to the application on launch.</param>
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var config = services.GetRequiredService<IConfiguration>();
                    // Add IdentityServer data
                    Core.Data.DbInitialize.InitializeIdentitySever(services, config["secretJwtKey"], true);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            host.Run();
        }

        /// <summary>
        /// Builds a default <see cref="IWebHost"/>.
        /// </summary>
        /// <param name="args">Any command-line arguments passed to the application on launch.</param>
        private static IWebHost BuildWebHost(string[] args)
            => WebHost.CreateDefaultBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Parse(URLs.Auth_IP), URLs.Auth_Port, config =>
                    {
                        config.UseHttps("localhost.pfx", "password");
                    });
                })
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();
    }
}
