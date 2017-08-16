using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
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
        public static void Main(string[] args) => BuildWebHost(args).Run();

        /// <summary>
        /// Builds a default <see cref="IWebHost"/>.
        /// </summary>
        /// <param name="args">Any command-line arguments passed to the application on launch.</param>
        private static IWebHost BuildWebHost(string[] args)
            => WebHost.CreateDefaultBuilder()
                .UseUrls(URLs.AuthURL)
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();
    }
}
