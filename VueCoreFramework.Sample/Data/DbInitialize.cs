using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using VueCoreFramework.Core.Data.Identity;
using VueCoreFramework.Core.Models;
using VueCoreFramework.Sample.Data;
using VueCoreFramework.Sample.Models;

namespace VueCoreFramework.Sample.Data
{
    /// <summary>
    /// Used to seed the application's database.
    /// </summary>
    public static class DbInitialize
    {
        /// <summary>
        /// Seeds the application's database with data.
        /// </summary>
        public static void Initialize(IServiceProvider provider)
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
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Switzerland\"}",
                        EpiIndex = 87.67,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            Name = "{\"default\":\"en-US\",\"en-US\":\"Doris Leuthard\"}",
                            Birthdate = new DateTime(1963, 4, 10),
                            TimeInOfficeTicks = 138240000000000,
                            Age = 54
                        }
                    },
                    new Country
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Luxembourg\"}",
                        EpiIndex = 83.29,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            Name = "{\"default\":\"en-US\",\"en-US\":\"Xavier Bettel\"}",
                            Birthdate = new DateTime(1973, 3, 3),
                            TimeInOfficeTicks = 1416960000000000,
                            Age = 44
                        }
                    },
                    new Country
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Australia\"}",
                        EpiIndex = 82.4,
                        FlagPrimaryColor = "#0000ff",
                        Leader = new Leader
                        {
                            Name = "{\"default\":\"en-US\",\"en-US\":\"Malcolm Turnbull\"}",
                            Birthdate = new DateTime(1954, 10, 24),
                            TimeInOfficeTicks = 716256000000000,
                            Age = 62
                        }
                    },
                    new Country
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Singapore\"}",
                        EpiIndex = 81.78,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            Name = "{\"default\":\"en-US\",\"en-US\":\"Lee Hsien Loong\"}",
                            Birthdate = new DateTime(1952, 2, 10),
                            TimeInOfficeTicks = 4158432000000000,
                            Age = 65
                        }
                    },
                    new Country
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Czech Republic\"}",
                        EpiIndex = 81.47,
                        FlagPrimaryColor = "#0000ff",
                        Leader = new Leader
                        {
                            Name = "{\"default\":\"en-US\",\"en-US\":\"Bohuslav Sobotka\"}",
                            Birthdate = new DateTime(1971, 10, 23),
                            TimeInOfficeTicks = 755136000000000,
                            Age = 45
                        }
                    },
                    new Country
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Germany\"}",
                        EpiIndex = 80.47,
                        FlagPrimaryColor = "#000000",
                        Leader = new Leader
                        {
                            Name = "{\"default\":\"en-US\",\"en-US\":\"Angela Merkel\"}",
                            Birthdate = new DateTime(1954, 7, 17),
                            TimeInOfficeTicks = 3930336000000000,
                            Age = 62
                        }
                    },
                    new Country
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Spain\"}",
                        EpiIndex = 79.09,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            Name = "{\"default\":\"en-US\",\"en-US\":\"Mariano Rajoy\"}",
                            Birthdate = new DateTime(1955, 3, 27),
                            TimeInOfficeTicks = 2062368000000000,
                            Age = 62
                        }
                    },
                    new Country
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Austria\"}",
                        EpiIndex = 78.32,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            Name = "{\"default\":\"en-US\",\"en-US\":\"Christian Kern\"}",
                            Birthdate = new DateTime(1966, 1, 4),
                            TimeInOfficeTicks = 349920000000000,
                            Age = 51
                        }
                    },
                    new Country
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Sweden\"}",
                        EpiIndex = 78.09,
                        FlagPrimaryColor = "#0000ff",
                        Leader = new Leader
                        {
                            Name = "{\"default\":\"en-US\",\"en-US\":\"Stefan Löfven\"}",
                            Birthdate = new DateTime(1957, 7, 21),
                            TimeInOfficeTicks = 1047168000000000,
                            Age = 59
                        }
                    },
                    new Country
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Norway\"}",
                        EpiIndex = 78.04,
                        FlagPrimaryColor = "#ff0000",
                        Leader = new Leader
                        {
                            Name = "{\"default\":\"en-US\",\"en-US\":\"Erna Solberg\"}",
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
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Bern\"}",
                        Population = 131554,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[0].Id,
                        IsCapitol = true
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Zürich\"}",
                        Population = 396027,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[0].Id
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Luxembourg City\"}",
                        Population = 115227,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[1].Id,
                        IsCapitol = true
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Canberra\"}",
                        Population = 422510,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[2].Id,
                        IsCapitol = true
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Sydney\"}",
                        Population = 4840628,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[2].Id
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Singapore\"}",
                        Population = 5607300,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[3].Id,
                        IsCapitol = true
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Prague\"}",
                        Population = 1259079,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[4].Id,
                        IsCapitol = true
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Brno\"}",
                        Population = 377440,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[4].Id
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Berlin\"}",
                        Population = 3421829,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[5].Id,
                        IsCapitol = true
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Hamburg\"}",
                        Population = 1746342,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[5].Id
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Madrid\"}",
                        Population = 3165235,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[6].Id,
                        IsCapitol = true
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Barcelona\"}",
                        Population = 1602386,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[6].Id
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Vienna\"}",
                        Population = 1812605,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[7].Id,
                        IsCapitol = true
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Graz\"}",
                        Population = 269997,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[7].Id
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Stockholm\"}",
                        Population = 1515017,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[8].Id,
                        IsCapitol = true
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Gothenburg\"}",
                        Population = 572799,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[8].Id
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Oslo\"}",
                        Population = 975744,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[9].Id,
                        IsCapitol = true
                    },
                    new City
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Bergen\"}",
                        Population = 252772,
                        LocalTimeAtGMTMidnight = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0),
                        Transit = CityTransit.Airport | CityTransit.BusStation | CityTransit.TrainDepot,
                        CountryId = countries[9].Id
                    }
                };
                context.Cities.AddRange(cities);
                context.SaveChanges();
            }

            if (!context.Airlines.Any())
            {
                var airlines = new Airline[]
                {
                    new Airline
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Lufthansa\"}",
                        International = true
                    },
                    new Airline
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"SAS\"}",
                        International = true
                    },
                    new Airline
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"International Airlines\"}",
                        International = true
                    },
                    new Airline
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Air Europa\"}"
                    },
                    new Airline
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Travel Service\"}",
                        International = false
                    },
                    new Airline
                    {
                        Name = "{\"default\":\"en-US\",\"en-US\":\"Luxair\"}"
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
