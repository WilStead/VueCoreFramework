using JSNLog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using System.Collections.Generic;
using VueCoreFramework.Core.Configuration;

namespace VueCoreFramework
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
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            services.AddMvc(options =>
            {
                options.SslPort = 44350;
                options.Filters.Add(new RequireHttpsAttribute());
            })
            .AddDataAnnotationsLocalization();

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
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var jsnlogConfiguration = new JsnlogConfiguration()
            {
                maxMessages = 5,
                serverSideMessageFormat = "Message: %message | url: %url",
                ajaxAppenders = new List<AjaxAppender>
                {
                    new AjaxAppender
                    {
                        name = "ajaxAppender",
                        storeInBufferLevel = "TRACE",
                        level = "INFO",
                        sendWithBufferLevel = "FATAL",
                        bufferSize = 20
                    }
                },
                loggers = new List<JSNLog.Logger>
                {
                    new JSNLog.Logger { appenders = "ajaxAppender" }
                }
            };
            if (env.IsDevelopment())
            {
                jsnlogConfiguration.consoleAppenders = new List<ConsoleAppender>
                {
                    new ConsoleAppender { name = "consoleAppender" }
                };
                jsnlogConfiguration.loggers[0].appenders = "ajaxAppender;consoleAppender";
            }
            app.UseJSNLog(new LoggingAdapter(loggerFactory), jsnlogConfiguration);

            var options = new RewriteOptions().AddRedirectToHttps();
            app.UseRewriter(options);

            app.UseCors("default");

            app.UseRequestLocalization(localization.Value);

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
