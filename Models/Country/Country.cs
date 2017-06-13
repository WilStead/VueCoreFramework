using MVCCoreVue.Data.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MVCCoreVue.Models
{
    [MenuClass(IconClass = "public")]
    public class Country : NamedDataItem
    {
        [Display(Prompt = "EPI Index")]
        [Required]
        [Range(0, 100)]
        [Step(0.01)]
        public double EpiIndex { get; set; }

        [Display(Name = "Primary flag color")]
        [DataType("Color")]
        public string FlagPrimaryColor { get; set; }

        [Display(AutoGenerateField = false)]
        public Guid? CapitolId { get; set; }

        [JsonIgnore]
        public City Capitol { get; set; }

        [JsonIgnore]
        public ICollection<City> Cities { get; set; } = new Collection<City>();

        [ForeignKey(nameof(Leader))]
        [Display(AutoGenerateField = false)]
        public Guid LeaderId { get; set; }

        [Required]
        [JsonIgnore]
        public Leader Leader { get; set; }

        [JsonIgnore]
        [Display(AutoGenerateField = false)]
        public ICollection<AirlineCountry> CountryAirlines { get; set; } = new Collection<AirlineCountry>();

        [NotMapped]
        [JsonIgnore]
        public List<Airline> Airlines => CountryAirlines?.Select(c => c.Airline)?.ToList();
    }
}
