using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    [Flags]
    public enum CityTransit
    {
        None = 0,
        Airport = 1,
        [Description("Bus Station")]
        BusStation = 2,
        [Description("Train Depot")]
        TrainDepot = 4
    }

    [MenuClass(Category = "Country")]
    public class City : DataItem
    {
        [Required]
        [Range(3, 25)]
        public string Name { get; set; }

        [Display(Prompt = "Local time at GMT midnight")]
        [DataType(DataType.Time)]
        public DateTime LocalTimeAtGMTMidnight { get; set; }

        [Range(0, int.MaxValue)]
        public int Population { get; set; }

        public CityTransit Transit { get; set; }

        public override string ToString() => Name;
    }
}
