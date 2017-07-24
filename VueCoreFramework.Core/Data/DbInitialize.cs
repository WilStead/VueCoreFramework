using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using VueCoreFramework.Core.Configuration;
using VueCoreFramework.Core.Data.Identity;
using VueCoreFramework.Core.Extensions;
using VueCoreFramework.Core.Models;

namespace VueCoreFramework.Core.Data
{
    /// <summary>
    /// Used to seed the framework's database.
    /// </summary>
    public static class DbInitialize
    {
        /// <summary>
        /// Seeds the framework's database with core data.
        /// </summary>
        public static void Initialize(IServiceProvider provider, VueCoreFrameworkDbContext context)
        {
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!context.Roles.Any(r => r.Name == CustomRoles.SiteAdmin))
            {
                var admin = new IdentityRole(CustomRoles.SiteAdmin);
                roleManager.CreateAsync(admin).Wait();
                roleManager.AddClaimAsync(admin, new Claim(CustomClaimTypes.PermissionGroupSiteAdmin, CustomClaimTypes.PermissionAll)).Wait();
                roleManager.AddClaimAsync(admin, new Claim(CustomClaimTypes.PermissionDataAll, CustomClaimTypes.PermissionAll)).Wait();
                context.SaveChanges();
            }
            if (!context.Roles.Any(r => r.Name == CustomRoles.Admin))
            {
                var admin = new IdentityRole(CustomRoles.Admin);
                roleManager.CreateAsync(admin).Wait();
                roleManager.AddClaimAsync(admin, new Claim(CustomClaimTypes.PermissionGroupAdmin, CustomClaimTypes.PermissionAll)).Wait();
                roleManager.AddClaimAsync(admin, new Claim(CustomClaimTypes.PermissionDataAll, CustomClaimTypes.PermissionAll)).Wait();
                context.SaveChanges();
            }

            var siteAdminRole = context.Roles.FirstOrDefault(r => r.Name == CustomRoles.SiteAdmin);
            if (!context.Users.Any(u => u.Roles.Any(r => r.RoleId == siteAdminRole.Id)))
            {
                var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
                var adminOptions = new AdminOptions();
                provider.GetRequiredService<IConfigureOptions<AdminOptions>>().Configure(adminOptions);
                var user = context.Users.FirstOrDefault(u => u.Email == adminOptions.AdminEmailAddress);
                if (user == null)
                {
                    user = new ApplicationUser { UserName = "Admin", Email = adminOptions.AdminEmailAddress };
                    userManager.CreateAsync(user, adminOptions.AdminPassword).Wait();
                    userManager.Users.FirstOrDefault().EmailConfirmed = true;
                    userManager.UpdateAsync(user).Wait();
                }
                userManager.AddToRoleAsync(user, CustomRoles.SiteAdmin).Wait();
                userManager.AddToRoleAsync(user, CustomRoles.Admin).Wait();
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Seeds the framework's database with IdentityServer data.
        /// </summary>
        public static void InitializeIdentitySever(IApplicationBuilder app, string secret, bool replace)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var urls = serviceScope.ServiceProvider.GetRequiredService<IOptions<URLOptions>>().Value;

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (replace)
                {
                    context.Clients.Clear();
                }
                if (replace || !context.Clients.Any())
                {
                    foreach (var client in IdentityServerConfig.GetClients(secret, urls))
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }
                if (replace)
                {
                    context.IdentityResources.Clear();
                }
                if (replace || !context.IdentityResources.Any())
                {
                    foreach (var resource in IdentityServerConfig.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
                if (replace)
                {
                    context.ApiResources.Clear();
                }
                if (replace || !context.ApiResources.Any())
                {
                    foreach (var resource in IdentityServerConfig.GetApiResources(secret))
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
