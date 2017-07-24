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
using VueCoreFramework.Core.Configuration;
using VueCoreFramework.Core.Models;
using VueCoreFramework.Core.Services;
using VueCoreFramework.Sample.Data;

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
            services.AddOptions();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
                config.SignIn.RequireConfirmedEmail = true;
                config.Cookies.ApplicationCookie.AutomaticChallenge = false;
                config.Cookies.ApplicationCookie.LoginPath = PathString.FromUriComponent("/Home/Index?forwardUrl=%2Flogin");
                config.Cookies.ApplicationCookie.AccessDeniedPath = PathString.FromUriComponent("/Home/Index?forwardUrl=%2Ferror%2F403");
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = LocalizationConfig.SupportedCultures;
                options.SupportedUICultures = LocalizationConfig.SupportedCultures;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddMvc(options =>
            {
                options.SslPort = 44325;
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
            services.Configure<URLOptions>(Configuration.GetSection("URLs"));
        }

        /// <summary>
        /// This method gets called by the runtime, and is used to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Provides the mechanisms to configure the application's request pipeline.</param>
        /// <param name="env">An <see cref="IHostingEnvironment"/> used to set up configuration sources.</param>
        /// <param name="loggerFactory">Used to configure the logging system.</param>
        /// <param name="localization">Specifies options for the <see cref="RequestLocalizationMiddleware"/>.</param>
        /// <param name="urls">Provides the URLs for the different hosts which form the application.</param>
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IOptions<RequestLocalizationOptions> localization,
            IOptions<URLOptions> urls)
        {
            loggerFactory.AddNLog();
            app.AddNLogWeb();
            LogManager.Configuration.Variables["connectionString"] = Configuration.GetConnectionString("DefaultConnection");
            LogManager.Configuration.Variables["logDir"] = Configuration["LogDir"];

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseExceptionHandler("/Error/500");
            }

            var options = new RewriteOptions().AddRedirectToHttps();
            app.UseRewriter(options);

            app.UseRequestLocalization(localization.Value);

            app.UseIdentity();

            app.UseCors("default");

            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                Authority = urls.Value.AuthURL,
                ApiName = IdentityServerConfig.apiName,
                ApiSecret = Configuration["secretJwtKey"],
                RequireHttpsMetadata = true
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "api/{controller}/{action}");
            });

            // Seed the database
            Core.Data.DbInitialize.Initialize(
                app.ApplicationServices,
                app.ApplicationServices.GetRequiredService<ApplicationDbContext>());

            // Add sample data
            DbInitialize.Initialize(app.ApplicationServices);
        }
    }
}
