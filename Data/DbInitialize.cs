using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MVCCoreVue.Models;
using MVCCoreVue.Services;
using System;
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
                    user = new ApplicationUser { UserName = adminOptions.AdminEmailAddress, Email = adminOptions.AdminEmailAddress };
                    userManager.CreateAsync(user, adminOptions.AdminPassword).Wait();
                    userManager.Users.FirstOrDefault().EmailConfirmed = true;
                    userManager.UpdateAsync(user).Wait();
                }
                userManager.AddToRoleAsync(user, CustomRoles.SiteAdmin).Wait();
                userManager.AddToRoleAsync(user, CustomRoles.Admin).Wait();
                context.SaveChanges();
            }

            if (!context.Countries.Any())
            {
                var countries = new Country[]
                {
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Switzerland",
                        EpiIndex = 87.67,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            CreationTimestamp = DateTime.Now,
                            UpdateTimestamp = DateTime.Now,
                            Name = "Doris Leuthard",
                            Birthdate = new DateTime(1963, 4, 10),
                            TimeInOfficeTicks = 138240000000000,
                            Age = 54
                        }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Luxembourg",
                        EpiIndex = 83.29,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            CreationTimestamp = DateTime.Now,
                            UpdateTimestamp = DateTime.Now,
                            Name = "Xavier Bettel",
                            Birthdate = new DateTime(1973, 3, 3),
                            TimeInOfficeTicks = 1416960000000000,
                            Age = 44
                        }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Australia",
                        EpiIndex = 82.4,
                        FlagPrimaryColor = "#0000ff",
                        Leader = new Leader
                        {
                            CreationTimestamp = DateTime.Now,
                            UpdateTimestamp = DateTime.Now,
                            Name = "Malcolm Turnbull",
                            Birthdate = new DateTime(1954, 10, 24),
                            TimeInOfficeTicks = 716256000000000,
                            Age = 62
                        }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Singapore",
                        EpiIndex = 81.78,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            CreationTimestamp = DateTime.Now,
                            UpdateTimestamp = DateTime.Now,
                            Name = "Lee Hsien Loong",
                            Birthdate = new DateTime(1952, 2, 10),
                            TimeInOfficeTicks = 4158432000000000,
                            Age = 65
                        }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Czech Republic",
                        EpiIndex = 81.47,
                        FlagPrimaryColor = "#0000ff",
                        Leader = new Leader
                        {
                            CreationTimestamp = DateTime.Now,
                            UpdateTimestamp = DateTime.Now,
                            Name = "Bohuslav Sobotka",
                            Birthdate = new DateTime(1971, 10, 23),
                            TimeInOfficeTicks = 755136000000000,
                            Age = 45
                        }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Germany",
                        EpiIndex = 80.47,
                        FlagPrimaryColor = "#000000",
                        Leader = new Leader
                        {
                            CreationTimestamp = DateTime.Now,
                            UpdateTimestamp = DateTime.Now,
                            Name = "Angela Merkel",
                            Birthdate = new DateTime(1954, 7, 17),
                            TimeInOfficeTicks = 3930336000000000,
                            Age = 62
                        }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Spain",
                        EpiIndex = 79.09,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            CreationTimestamp = DateTime.Now,
                            UpdateTimestamp = DateTime.Now,
                            Name = "Mariano Rajoy",
                            Birthdate = new DateTime(1955, 3, 27),
                            TimeInOfficeTicks = 2062368000000000,
                            Age = 62
                        }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Austria",
                        EpiIndex = 78.32,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            CreationTimestamp = DateTime.Now,
                            UpdateTimestamp = DateTime.Now,
                            Name = "Christian Kern",
                            Birthdate = new DateTime(1966, 1, 4),
                            TimeInOfficeTicks = 349920000000000,
                            Age = 51
                        }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Sweden",
                        EpiIndex = 78.09,
                        FlagPrimaryColor = "#0000ff",
                        Leader = new Leader
                        {
                            CreationTimestamp = DateTime.Now,
                            UpdateTimestamp = DateTime.Now,
                            Name = "Stefan Löfven",
                            Birthdate = new DateTime(1957, 7, 21),
                            TimeInOfficeTicks = 1047168000000000,
                            Age = 59
                        }
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Norway",
                        EpiIndex = 78.04,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            CreationTimestamp = DateTime.Now,
                            UpdateTimestamp = DateTime.Now,
                            Name = "Erna Solberg",
                            Birthdate = new DateTime(1961, 2, 24),
                            TimeInOfficeTicks = 1374624000000000,
                            Age = 56
                        }
                    }
                };
                context.Countries.AddRange(countries);
                context.SaveChanges();
            }

            if (!context.Cities.Any())
            {
                var countries = context.Countries.ToList();
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
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[0].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Zürich",
                        Population = 396027,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[0].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Luxembourg City",
                        Population = 115227,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[1].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Canberra",
                        Population = 422510,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[2].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Sydney",
                        Population = 4840628,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[2].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Singapore",
                        Population = 5607300,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[3].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Prague",
                        Population = 1259079,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[4].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Brno",
                        Population = 377440,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[4].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Berlin",
                        Population = 3421829,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[5].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Hamburg",
                        Population = 1746342,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[5].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Madrid",
                        Population = 3165235,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[6].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Barcelona",
                        Population = 1602386,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[6].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Vienna",
                        Population = 1812605,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[7].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Graz",
                        Population = 269997,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[7].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Stockholm",
                        Population = 1515017,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[8].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Gothenburg",
                        Population = 572799,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[8].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Oslo",
                        Population = 975744,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[9].Id
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Bergen",
                        Population = 252772,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[9].Id
                    }
            };
                context.Cities.AddRange(cities);
                context.SaveChanges();
                var cityList = context.Cities.ToList();
                countries[0].Capitol = cityList[0];
                countries[1].Capitol = cityList[2];
                countries[2].Capitol = cityList[3];
                countries[3].Capitol = cityList[5];
                countries[4].Capitol = cityList[6];
                countries[5].Capitol = cityList[8];
                countries[6].Capitol = cityList[10];
                countries[7].Capitol = cityList[12];
                countries[8].Capitol = cityList[14];
                countries[9].Capitol = cityList[16];
                context.SaveChanges();
            }
        }
    }
}
