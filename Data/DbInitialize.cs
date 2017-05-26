using MVCCoreVue.Models;
using System;
using System.Linq;

namespace MVCCoreVue.Data
{
    public static class DbInitialize
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Countries.Any())
            {
                return;
            }

            var countries = new Country[]
            {
                new Country
                {
                    CreationTimestamp = DateTime.Now,
                    UpdateTimestamp = DateTime.MinValue,
                    Name = "Switzerland",
                    EpiIndex = 87.67
                },
                new Country
                {
                    CreationTimestamp = DateTime.Now,
                    UpdateTimestamp = DateTime.MinValue,
                    Name = "Luxembourg",
                    EpiIndex = 83.29
                },
                new Country
                {
                    CreationTimestamp = DateTime.Now,
                    UpdateTimestamp = DateTime.MinValue,
                    Name = "Australia",
                    EpiIndex = 82.4
                },
                new Country
                {
                    CreationTimestamp = DateTime.Now,
                    UpdateTimestamp = DateTime.MinValue,
                    Name = "Singapore",
                    EpiIndex = 81.78
                },
                new Country
                {
                    CreationTimestamp = DateTime.Now,
                    UpdateTimestamp = DateTime.MinValue,
                    Name = "Czech Republic",
                    EpiIndex = 81.47
                },
                new Country
                {
                    CreationTimestamp = DateTime.Now,
                    UpdateTimestamp = DateTime.MinValue,
                    Name = "Germany",
                    EpiIndex = 80.47
                },
                new Country
                {
                    CreationTimestamp = DateTime.Now,
                    UpdateTimestamp = DateTime.MinValue,
                    Name = "Spain",
                    EpiIndex = 79.09
                },
                new Country
                {
                    CreationTimestamp = DateTime.Now,
                    UpdateTimestamp = DateTime.MinValue,
                    Name = "Austria",
                    EpiIndex = 78.32
                },
                new Country
                {
                    CreationTimestamp = DateTime.Now,
                    UpdateTimestamp = DateTime.MinValue,
                    Name = "Sweden",
                    EpiIndex = 78.09
                },
                new Country
                {
                    CreationTimestamp = DateTime.Now,
                    UpdateTimestamp = DateTime.MinValue,
                    Name = "Norway",
                    EpiIndex = 78.04
                }
            };
            context.Countries.AddRange(countries);
            context.SaveChanges();
        }
    }
}
