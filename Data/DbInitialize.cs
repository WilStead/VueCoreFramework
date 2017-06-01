using MVCCoreVue.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MVCCoreVue.Data
{
    public static class DbInitialize
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (!context.Cities.Any())
            {
                var cities = new City[]
                {
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Bern",
                        Population = 131554
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Zürich",
                        Population = 396027
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Luxembourg City",
                        Population = 115227
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Canberra",
                        Population = 422510
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Sydney",
                        Population = 4840628
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Singapore",
                        Population = 5607300
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Prague",
                        Population = 1259079
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Brno",
                        Population = 377440
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Berlin",
                        Population = 3421829
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Hamburg",
                        Population = 1746342
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Madrid",
                        Population = 3165235
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Barcelona",
                        Population = 1602386
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Vienna",
                        Population = 1812605
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Graz",
                        Population = 269997
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Stockholm",
                        Population = 1515017
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Gothenburg",
                        Population = 572799
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Oslo",
                        Population = 975744
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Bergen",
                        Population = 252772
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
                        Age = 54
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Xavier Bettel",
                        Age = 44
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Malcolm Turnbull",
                        Age = 62
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Lee Hsien Loong",
                        Age = 65
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Bohuslav Sobotka",
                        Age = 45
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Angela Merkel",
                        Age = 62
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Mariano Rajoy",
                        Age = 62
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Christian Kern",
                        Age = 51
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Stefan Löfven",
                        Age = 59
                    },
                    new Leader
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.Now,
                        Name = "Erna Solberg",
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
                        Cities = new Collection<City> { cityList[16], cityList[17] }
                    }
                };
                context.Countries.AddRange(countries);
                context.SaveChanges();
            }
        }
    }
}
