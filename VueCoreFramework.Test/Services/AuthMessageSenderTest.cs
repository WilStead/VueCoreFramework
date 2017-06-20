using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading.Tasks;
using VueCoreFramework.Services;

namespace VueCoreFramework.Test.Services
{
    [TestClass]
    public class AuthMessageSenderTest
    {
        [TestMethod]
        public async Task SendEmailAsyncTest()
        {
            var assembly = typeof(Startup).GetTypeInfo().Assembly;
            var contentRoot = TestHelper.GetProjectPath(assembly);
            var config = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddUserSecrets<Startup>()
                .AddEnvironmentVariables()
                .Build();

            var sp = new ServiceCollection()
                .AddLogging()
                .Configure<AuthMessageSenderOptions>(config.GetSection("AuthMessageSender"))
                .AddOptions()
                .BuildServiceProvider();

            var optionsAccessor = sp.GetService<IOptions<AuthMessageSenderOptions>>();

            var factory = sp.GetService<ILoggerFactory>();
            factory.AddConsole();
            ILogger<AuthMessageSender> logger = factory.CreateLogger<AuthMessageSender>();

            var cls = new AuthMessageSender(optionsAccessor, logger);

            // TODO: Replace placeholder code with commented code to test email function when real
            //       email info is available.

            await Task.Run(() => { });

            //var email = "test@example.com";
            //var subject = "Test";
            //var message = "Test";
            //try
            //{
            //    await cls.SendEmailAsync(email, subject, message);
            //}
            //catch (Exception ex)
            //{
            //    Assert.Fail(ex.Message);
            //}
        }
    }
}
