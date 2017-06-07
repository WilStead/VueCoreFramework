using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    [Flags]
    public enum CityTransit
    {
        None = 0,
        Airport = 1,
        BusStation = 2,
        TrainDepot = 4
    }

    [MenuClass(Category = "Country")]
    public class City : DataItem
    {
        [Required]
        [Range(3, 25)]
        public string Name { get; set; }

        [Range(0, int.MaxValue)]
        public int Population { get; set; }

        public CityTransit Transit { get; set; }

        public override string ToString() => Name;
    }
}
