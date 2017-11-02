using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
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
        /// The <see cref="IConfiguration"/> object.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="Startup"/>.
        /// </summary>
        /// <param name="env">An <see cref="IHostingEnvironment"/> used to set up configuration sources.</param>
        /// <param name="configuration">An <see cref="IConfiguration"/> which will be exposed as a class Property.</param>
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;

            env.ConfigureNLog("nlog.config");
        }

        /// <summary>
        /// This method gets called by the runtime, and is used to add services to the container.
        /// </summary>
        /// <param name="services">A collection of service descriptors.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            var urls = Configuration.GetSection("URLs");

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
                config.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            //services.ConfigureApplicationCookie(options =>
            //{
            //    options.LoginPath = PathString.FromUriComponent("/Home/Index?forwardUrl=%2Flogin");
            //    options.AccessDeniedPath = PathString.FromUriComponent("/Home/Index?forwardUrl=%2Ferror%2F403");

            //    // Disable automatic challenge and allow 401 responses.
            //    options.Events = new CookieAuthenticationEvents
            //    {
            //        OnRedirectToLogin = ctx =>
            //        {
            //            if (ctx.Response.StatusCode == (int)HttpStatusCode.OK)
            //            {
            //                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //            }
            //            else
            //            {
            //                ctx.Response.Redirect(ctx.RedirectUri);
            //            }
            //            return Task.FromResult(0);
            //        }
            //    };
            //});
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = urls.GetValue("AuthURL", $"https://{new IPEndPoint(IPAddress.Parse(URLs.Auth_IP), URLs.Auth_Port).ToString()}/");
                    options.ApiName = IdentityServerConfig.apiName;
                    options.ApiSecret = Configuration["secretJwtKey"];
                    options.EnableCaching = true;
                });

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
                        .WithOrigins(urls["AuthURL"].TrimEnd('/'), urls["ClientURL"].TrimEnd('/'))
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
                options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration.GetSection("AuthMessageSender"));
            services.Configure<AdminOptions>(Configuration.GetSection("AdminOptions"));
            services.Configure<URLOptions>(urls);
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

            app.UseCors("default");

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "api/{controller}/{action}");
            });
        }
    }
}
