using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using VueCoreFramework.Core.Configuration;

namespace VueCoreFramework.API
{
    /// <summary>
    /// The main class of the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        /// <param name="args">Any commandline arguments passed to the application on launch.</param>
        public static void Main(string[] args)
        {
            var cert = new X509Certificate2("localhost.pfx", "password");

            var host = new WebHostBuilder()
                .UseKestrel(options => options.UseHttps(cert))
                .UseUrls(URLs.ApiURL)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
