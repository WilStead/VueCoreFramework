using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MVCCoreVue.Models;
using MVCCoreVue.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;

namespace MVCCoreVue.Data
{
    public static class DbInitialize
    {
        public static void Initialize(IServiceProvider provider)
        {
            var context = provider.GetRequiredService<ApplicationDbContext>();

            if (!context.Roles.Any(r => r.Name == CustomRoles.SiteAdmin))
            {
                var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
                var admin = new IdentityRole(CustomRoles.SiteAdmin);
                roleManager.CreateAsync(admin).Wait();
                roleManager.AddClaimAsync(admin, new Claim(CustomClaimTypes.PermissionGroupSiteAdmin, CustomClaimTypes.PermissionAll)).Wait();
                roleManager.AddClaimAsync(admin, new Claim(CustomClaimTypes.PermissionDataAll, CustomClaimTypes.PermissionAll)).Wait();
                context.SaveChanges();
            }
            if (!context.Roles.Any(r => r.Name == CustomRoles.Admin))
            {
                var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
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
                    user = new ApplicationUser { Email = adminOptions.AdminEmailAddress };
                    userManager.CreateAsync(user, adminOptions.AdminPassword).Wait();
                    userManager.Users.FirstOrDefault().EmailConfirmed = true;
                }
                userManager.AddToRoleAsync(user, CustomRoles.SiteAdmin);
                context.SaveChanges();
            }

            if (!context.Cities.Any())
            {

                var now = DateTime.Now;
                    var cities = new City[]
                {
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Bern",
                        Population = 131554,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Zürich",
                        Population = 396027,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Luxembourg City",
                        Population = 115227,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Canberra",
                        Population = 422510,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Sydney",
                        Population = 4840628,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Singapore",
                        Population = 5607300,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Prague",
                        Population = 1259079,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Brno",
                        Population = 377440,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Berlin",
                        Population = 3421829,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Hamburg",
                        Population = 1746342,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Madrid",
                        Population = 3165235,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Barcelona",
                        Population = 1602386,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Vienna",
                        Population = 1812605,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Graz",
                        Population = 269997,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Stockholm",
                        Population = 1515017,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Gothenburg",
                        Population = 572799,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Oslo",
                        Population = 975744,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Bergen",
                        Population = 252772,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot
                    }
                };
                context.Cities.AddRange(cities);
                context.SaveChanges();
            }

            if (!context.Leaders.Any())
            {
                var leaders = new Leader[]
                {
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Doris Leuthard",
                        Birthdate = new DateTime(1963, 4, 10),
                        Age = 54
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Xavier Bettel",
                        Birthdate = new DateTime(1973, 3, 3),
                        Age = 44
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Malcolm Turnbull",
                        Birthdate = new DateTime(1954, 10, 24),
                        Age = 62
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Lee Hsien Loong",
                        Birthdate = new DateTime(1952, 2, 10),
                        Age = 65
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Bohuslav Sobotka",
                        Birthdate = new DateTime(1971, 10, 23),
                        Age = 45
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Angela Merkel",
                        Birthdate = new DateTime(1954, 7, 17),
                        Age = 62
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Mariano Rajoy",
                        Birthdate = new DateTime(1955, 3, 27),
                        Age = 62
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Christian Kern",
                        Birthdate = new DateTime(1966, 1, 4),
                        Age = 51
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Stefan Löfven",
                        Birthdate = new DateTime(1957, 7, 21),
                        Age = 59
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Erna Solberg",
                        Birthdate = new DateTime(1961, 2, 24),
                        Age = 56
                    }
                };
                context.Leaders.AddRange(leaders);
                context.SaveChanges();
            }

            if (!context.Countries.Any())
            {
                var cityList = context.Cities.ToList();
                var leaderList = context.Leaders.ToList();
                var countries = new Country[]
                {
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Switzerland",
                        EpiIndex = 87.67,
                        CapitolId = cityList[0].Id,
                        LeaderId = leaderList[0].Id,
                        FlagPrimaryColor = "#ff0000",
                        Cities = new Collection<City> { cityList[0], cityList[1] }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Luxembourg",
                        EpiIndex = 83.29,
                        CapitolId = cityList[2].Id,
                        LeaderId = leaderList[1].Id,
                        FlagPrimaryColor = "#ff0000",
                        Cities = new Collection<City> { cityList[2] }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Australia",
                        EpiIndex = 82.4,
                        CapitolId = cityList[3].Id,
                        LeaderId = leaderList[2].Id,
                        FlagPrimaryColor = "#0000ff",
                        Cities = new Collection<City> { cityList[3], cityList[4] }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Singapore",
                        EpiIndex = 81.78,
                        CapitolId = cityList[5].Id,
                        LeaderId = leaderList[3].Id,
                        FlagPrimaryColor = "#ff0000",
                        Cities = new Collection<City> { cityList[5] }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Czech Republic",
                        EpiIndex = 81.47,
                        CapitolId = cityList[6].Id,
                        LeaderId = leaderList[4].Id,
                        FlagPrimaryColor = "#0000ff",
                        Cities = new Collection<City> { cityList[6], cityList[7] }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Germany",
                        EpiIndex = 80.47,
                        CapitolId = cityList[8].Id,
                        LeaderId = leaderList[5].Id,
                        FlagPrimaryColor = "#000000",
                        Cities = new Collection<City> { cityList[8], cityList[9] }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Spain",
                        EpiIndex = 79.09,
                        CapitolId = cityList[10].Id,
                        LeaderId = leaderList[6].Id,
                        FlagPrimaryColor = "#ff0000",
                        Cities = new Collection<City> { cityList[10], cityList[11] }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Austria",
                        EpiIndex = 78.32,
                        CapitolId = cityList[12].Id,
                        LeaderId = leaderList[7].Id,
                        FlagPrimaryColor = "#ff0000",
                        Cities = new Collection<City> { cityList[12], cityList[13] }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Sweden",
                        EpiIndex = 78.09,
                        CapitolId = cityList[14].Id,
                        LeaderId = leaderList[8].Id,
                        FlagPrimaryColor = "#0000ff",
                        Cities = new Collection<City> { cityList[14], cityList[15] }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Norway",
                        EpiIndex = 78.04,
                        CapitolId = cityList[16].Id,
                        LeaderId = leaderList[9].Id,
                        FlagPrimaryColor = "#ff0000",
                        Cities = new Collection<City> { cityList[16], cityList[17] }
                    }
                };
                context.Countries.AddRange(countries);
                context.SaveChanges();
            }
        }
    }
}
