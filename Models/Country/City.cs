using MVCCoreVue.Data.Attributes;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public class City : NamedDataItem
    {
        [Display(Prompt = "Local time at GMT midnight")]
        [DataType(DataType.Time)]
        public DateTime LocalTimeAtGMTMidnight { get; set; }

        [Range(0, int.MaxValue)]
        public int Population { get; set; }

        public CityTransit Transit { get; set; }

        [Display(AutoGenerateField = false)]
        public Guid CitiesCountryId { get; set; }

        [JsonIgnore]
        [InverseProperty(nameof(Country.Cities))]
        [Display(AutoGenerateField = false)]
        public virtual Country CitiesCountry { get; set; }
    }
}
