using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VueCoreFramework.Models;
using VueCoreFramework.Services;
using System;
using System.Linq;
using System.Security.Claims;

namespace VueCoreFramework.Data
{
    /// <summary>
    /// Used to seed the application's database.
    /// </summary>
    public static class DbInitialize
    {
        /// <summary>
        /// Seeds the application's database.
        /// </summary>
        public static void Initialize(IServiceProvider provider)
        {
            var context = provider.GetRequiredService<ApplicationDbContext>();

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
        /// Seeds the application's database with sample data.
        /// </summary>
        public static void InitializeSampleData(IServiceProvider provider)
        {
            var context = provider.GetRequiredService<ApplicationDbContext>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!context.Users.Any(u => !u.Roles.Any()))
            {
                var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = context.Users.FirstOrDefault(u => u.Email == "test_user@example.com");
                if (user == null)
                {
                    user = new ApplicationUser { UserName = "Test_User", Email = "test_user@example.com" };
                    userManager.CreateAsync(user, "Password*1").Wait();
                    user = userManager.Users.FirstOrDefault(u => u.UserName == "Test_User");
                    user.EmailConfirmed = true;
                    userManager.UpdateAsync(user).Wait();
                }
                user = context.Users.FirstOrDefault(u => u.Email == "test_user_2@example.com");
                if (user == null)
                {
                    user = new ApplicationUser { UserName = "Test_User_2", Email = "test_user_2@example.com" };
                    userManager.CreateAsync(user, "Password*2").Wait();
                    user = userManager.Users.FirstOrDefault(u => u.UserName == "Test_User_2");
                    user.EmailConfirmed = true;
                    userManager.UpdateAsync(user).Wait();
                }
                context.SaveChanges();
            }

            if (!context.Countries.Any())
            {
                var countries = new Country[]
                {
                    new Country
                    {
                        Name = "Switzerland",
                        EpiIndex = 87.67,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            Name = "Doris Leuthard",
                            Birthdate = new DateTime(1963, 4, 10),
                            TimeInOfficeTicks = 138240000000000,
                            Age = 54
                        }
                    },
                    new Country
                    {
                        Name = "Luxembourg",
                        EpiIndex = 83.29,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            Name = "Xavier Bettel",
                            Birthdate = new DateTime(1973, 3, 3),
                            TimeInOfficeTicks = 1416960000000000,
                            Age = 44
                        }
                    },
                    new Country
                    {
                        Name = "Australia",
                        EpiIndex = 82.4,
                        FlagPrimaryColor = "#0000ff",
                        Leader = new Leader
                        {
                            Name = "Malcolm Turnbull",
                            Birthdate = new DateTime(1954, 10, 24),
                            TimeInOfficeTicks = 716256000000000,
                            Age = 62
                        }
                    },
                    new Country
                    {
                        Name = "Singapore",
                        EpiIndex = 81.78,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            Name = "Lee Hsien Loong",
                            Birthdate = new DateTime(1952, 2, 10),
                            TimeInOfficeTicks = 4158432000000000,
                            Age = 65
                        }
                    },
                    new Country
                    {
                        Name = "Czech Republic",
                        EpiIndex = 81.47,
                        FlagPrimaryColor = "#0000ff",
                        Leader = new Leader
                        {
                            Name = "Bohuslav Sobotka",
                            Birthdate = new DateTime(1971, 10, 23),
                            TimeInOfficeTicks = 755136000000000,
                            Age = 45
                        }
                    },
                    new Country
                    {
                        Name = "Germany",
                        EpiIndex = 80.47,
                        FlagPrimaryColor = "#000000",
                        Leader = new Leader
                        {
                            Name = "Angela Merkel",
                            Birthdate = new DateTime(1954, 7, 17),
                            TimeInOfficeTicks = 3930336000000000,
                            Age = 62
                        }
                    },
                    new Country
                    {
                        Name = "Spain",
                        EpiIndex = 79.09,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            Name = "Mariano Rajoy",
                            Birthdate = new DateTime(1955, 3, 27),
                            TimeInOfficeTicks = 2062368000000000,
                            Age = 62
                        }
                    },
                    new Country
                    {
                        Name = "Austria",
                        EpiIndex = 78.32,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            Name = "Christian Kern",
                            Birthdate = new DateTime(1966, 1, 4),
                            TimeInOfficeTicks = 349920000000000,
                            Age = 51
                        }
                    },
                    new Country
                    {
                        Name = "Sweden",
                        EpiIndex = 78.09,
                        FlagPrimaryColor = "#0000ff",
                        Leader = new Leader
                        {
                            Name = "Stefan Löfven",
                            Birthdate = new DateTime(1957, 7, 21),
                            TimeInOfficeTicks = 1047168000000000,
                            Age = 59
                        }
                    },
                    new Country
                    {
                        Name = "Norway",
                        EpiIndex = 78.04,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
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
                        Name = "Bern",
                        Population = 131554,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[0].Id
                    },
                    new City
                    {
                        Name = "Zürich",
                        Population = 396027,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[0].Id
                    },
                    new City
                    {
                        Name = "Luxembourg City",
                        Population = 115227,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[1].Id
                    },
                    new City
                    {
                        Name = "Canberra",
                        Population = 422510,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[2].Id
                    },
                    new City
                    {
                        Name = "Sydney",
                        Population = 4840628,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[2].Id
                    },
                    new City
                    {
                        Name = "Singapore",
                        Population = 5607300,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[3].Id
                    },
                    new City
                    {
                        Name = "Prague",
                        Population = 1259079,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[4].Id
                    },
                    new City
                    {
                        Name = "Brno",
                        Population = 377440,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[4].Id
                    },
                    new City
                    {
                        Name = "Berlin",
                        Population = 3421829,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[5].Id
                    },
                    new City
                    {
                        Name = "Hamburg",
                        Population = 1746342,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[5].Id
                    },
                    new City
                    {
                        Name = "Madrid",
                        Population = 3165235,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[6].Id
                    },
                    new City
                    {
                        Name = "Barcelona",
                        Population = 1602386,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[6].Id
                    },
                    new City
                    {
                        Name = "Vienna",
                        Population = 1812605,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[7].Id
                    },
                    new City
                    {
                        Name = "Graz",
                        Population = 269997,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[7].Id
                    },
                    new City
                    {
                        Name = "Stockholm",
                        Population = 1515017,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[8].Id
                    },
                    new City
                    {
                        Name = "Gothenburg",
                        Population = 572799,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[8].Id
                    },
                    new City
                    {
                        Name = "Oslo",
                        Population = 975744,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[9].Id
                    },
                    new City
                    {
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

            if (!context.Airlines.Any())
            {
                var airlines = new Airline[]
                {
                    new Airline
                    {
                        Name = "Lufthansa",
                        International = true
                    },
                    new Airline
                    {
                        Name = "SAS",
                        International = true
                    },
                    new Airline
                    {
                        Name = "International Airlines",
                        International = true
                    },
                    new Airline
                    {
                        Name = "Air Europa"
                    },
                    new Airline
                    {
                        Name = "Travel Service",
                        International = false
                    },
                    new Airline
                    {
                        Name = "Luxair"
                    }
                };
                context.Airlines.AddRange(airlines);
                context.SaveChanges();
                var countries = context.Countries.ToList();
                var airlineList = context.Airlines.ToList();
                airlineList[0].Countries.Add(new AirlineCountry
                {
                    Airline = airlineList[0],
                    Country = countries[0]
                });
                airlineList[0].Countries.Add(new AirlineCountry
                {
                    Airline = airlineList[0],
                    Country = countries[5]
                });
                airlineList[0].Countries.Add(new AirlineCountry
                {
                    Airline = airlineList[0],
                    Country = countries[7]
                });
                airlineList[1].Countries.Add(new AirlineCountry
                {
                    Airline = airlineList[1],
                    Country = countries[8]
                });
                airlineList[1].Countries.Add(new AirlineCountry
                {
                    Airline = airlineList[1],
                    Country = countries[9]
                });
                airlineList[2].Countries.Add(new AirlineCountry
                {
                    Airline = airlineList[2],
                    Country = countries[6]
                });
                airlineList[3].Countries.Add(new AirlineCountry
                {
                    Airline = airlineList[3],
                    Country = countries[6]
                });
                airlineList[4].Countries.Add(new AirlineCountry
                {
                    Airline = airlineList[4],
                    Country = countries[4]
                });
                airlineList[5].Countries.Add(new AirlineCountry
                {
                    Airline = airlineList[5],
                    Country = countries[1]
                });
            }

            if (!context.Roles.Any(r => r.Name == CustomRoles.AllUsers))
            {
                var allUsers = new IdentityRole(CustomRoles.AllUsers);
                roleManager.CreateAsync(allUsers).Wait();
                roleManager.AddClaimAsync(allUsers, new Claim(CustomClaimTypes.PermissionDataAll, nameof(Leader))).Wait();
                roleManager.AddClaimAsync(allUsers, new Claim(CustomClaimTypes.PermissionDataView, nameof(City))).Wait();
                roleManager.AddClaimAsync(allUsers, new Claim(CustomClaimTypes.PermissionDataEdit, nameof(City))).Wait();
                roleManager.AddClaimAsync(allUsers, new Claim(CustomClaimTypes.PermissionDataView, nameof(Country))).Wait();
                var countries = context.Countries.ToList();
                roleManager.AddClaimAsync(allUsers, new Claim(CustomClaimTypes.PermissionDataView, $"{nameof(Country)}{{{countries[0].Id}}}")).Wait();
                roleManager.AddClaimAsync(allUsers, new Claim(CustomClaimTypes.PermissionDataEdit, $"{nameof(Country)}{{{countries[0].Id}}}")).Wait();
                roleManager.AddClaimAsync(allUsers, new Claim(CustomClaimTypes.PermissionDataView, $"{nameof(Country)}{{{countries[1].Id}}}")).Wait();
                roleManager.AddClaimAsync(allUsers, new Claim(CustomClaimTypes.PermissionDataEdit, $"{nameof(Country)}{{{countries[1].Id}}}")).Wait();
                roleManager.AddClaimAsync(allUsers, new Claim(CustomClaimTypes.PermissionDataView, $"{nameof(Country)}{{{countries[2].Id}}}")).Wait();
                roleManager.AddClaimAsync(allUsers, new Claim(CustomClaimTypes.PermissionDataEdit, $"{nameof(Country)}{{{countries[2].Id}}}")).Wait();
                var airlines = context.Airlines.ToList();
                roleManager.AddClaimAsync(allUsers, new Claim(CustomClaimTypes.PermissionDataView, $"{nameof(Airline)}{{{airlines[0].Id}}}")).Wait();
                roleManager.AddClaimAsync(allUsers, new Claim(CustomClaimTypes.PermissionDataView, $"{nameof(Airline)}{{{airlines[1].Id}}}")).Wait();
                context.SaveChanges();
            }
        }
    }
}
