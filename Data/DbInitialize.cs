using MVCCoreVue.Models;
using System;
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
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Bern",
                        Population = 406900
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Luxembourg City",
                        Population = 115227
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Canberra",
                        Population = 381488
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Singapore",
                        Population = 5607300
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Prague",
                        Population = 1280508
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Berlin",
                        Population = 3671000
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Madrid",
                        Population = 3141991
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Vienna",
                        Population = 2600000
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Stockholm",
                        Population = 932917
                    },
                    new City
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Oslo",
                        Population = 1717900
                    }
                };
                context.Cities.AddRange(cities);
                context.SaveChanges();
            }

            if (!context.Countries.Any())
            {
                var cityList = context.Cities.ToList();
                var countries = new Country[]
                {
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Switzerland",
                        EpiIndex = 87.67,
                        CityId = cityList[0].Id
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Luxembourg",
                        EpiIndex = 83.29,
                        CityId = cityList[1].Id
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Australia",
                        EpiIndex = 82.4,
                        CityId = cityList[2].Id
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Singapore",
                        EpiIndex = 81.78,
                        CityId = cityList[3].Id
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Czech Republic",
                        EpiIndex = 81.47,
                        CityId = cityList[4].Id
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Germany",
                        EpiIndex = 80.47,
                        CityId = cityList[5].Id
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Spain",
                        EpiIndex = 79.09,
                        CityId = cityList[6].Id
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Austria",
                        EpiIndex = 78.32,
                        CityId = cityList[7].Id
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Sweden",
                        EpiIndex = 78.09,
                        CityId = cityList[8].Id
                    },
                    new Country
                    {
                        CreationTimestamp = DateTime.Now,
                        UpdateTimestamp = DateTime.MinValue,
                        Name = "Norway",
                        EpiIndex = 78.04,
                        CityId = cityList[9].Id
                    }
                };
                context.Countries.AddRange(countries);
                context.SaveChanges();
            }
        }
    }
}
