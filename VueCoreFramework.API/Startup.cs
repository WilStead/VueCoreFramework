using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using System.Globalization;
using VueCoreFramework.Core.Configuration;
using VueCoreFramework.Core.Data;
using VueCoreFramework.Core.Models;
using VueCoreFramework.Core.Services;

namespace VueCoreFramework.API
{
    /// <summary>
    /// Configures services and the application's request pipeline.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Startup"/>.
        /// </summary>
        /// <param name="env">An <see cref="IHostingEnvironment"/> used to set up configuration sources.</param>
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            env.ConfigureNLog("nlog.config");
        }

        /// <summary>
        /// The root of the configuration hierarchy.
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime, and is used to add services to the container.
        /// </summary>
        /// <param name="services">A collection of service descriptors.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddOptions();

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
                config.SignIn.RequireConfirmedEmail = true;
                config.Cookies.ApplicationCookie.AutomaticChallenge = false;
                config.Cookies.ApplicationCookie.LoginPath = new PathString("/Home/Index?forwardUrl=%2Flogin");
                config.Cookies.ApplicationCookie.AccessDeniedPath = new PathString("/Home/Index?forwardUrl=%2Ferror%2F403");
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            var supportedCultures = new[]
            {
                new CultureInfo("en-US")
            };
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.AddMvc(options =>
            {
                options.SslPort = 44393;
                options.Filters.Add(new RequireHttpsAttribute());
            })
            .AddDataAnnotationsLocalization();

            services.AddApiVersioning(options =>
            {
                options.ApiVersionReader = new MediaTypeApiVersionReader();
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration.GetSection("AuthMessageSender"));
            services.Configure<AdminOptions>(Configuration.GetSection("AdminOptions"));
        }

        /// <summary>
        /// This method gets called by the runtime, and is used to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Provides the mechanisms to configure the application's request pipeline.</param>
        /// <param name="env">An <see cref="IHostingEnvironment"/> used to set up configuration sources.</param>
        /// <param name="loggerFactory">Used to configure the logging system.</param>
        /// <param name="context">The application's Entity Framework database context.</param>
        /// <param name="localization">Specifies options for the <see cref="RequestLocalizationMiddleware"/>.</param>
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            ApplicationDbContext context,
            IOptions<RequestLocalizationOptions> localization)
        {
            loggerFactory.AddNLog();
            app.AddNLogWeb();
            LogManager.Configuration.Variables["connectionString"] = Configuration.GetConnectionString("DefaultConnection");
            LogManager.Configuration.Variables["logDir"] = Configuration["LogDir"];

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseExceptionHandler("/Error/500");
            }

            var options = new RewriteOptions().AddRedirectToHttps();
            app.UseRewriter(options);

            app.UseRequestLocalization(localization.Value);

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "api/{controller}/{action}");
            });

            // Seed the database
            DbInitialize.Initialize(app.ApplicationServices);

            // Add IdentityServer data
            DbInitialize.InitializeIdentitySever(app, Configuration["secretJwtKey"]);

            // Add sample data
            DbInitialize.InitializeSampleData(app.ApplicationServices);
        }
    }
}
