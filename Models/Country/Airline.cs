using MVCCoreVue.Data.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    [MenuClass(Category = "Country")]
    public class Airline : NamedDataItem
    {
        [JsonIgnore]
        [Display(AutoGenerateField = false)]
        public ICollection<AirlineCountry> AirlineCountries { get; set; } = new Collection<AirlineCountry>();
    }

    public class AirlineCountry
    {
        public Guid AirlineId { get; set; }
        public Airline Airline { get; set; }

        public Guid CountryId { get; set; }
        public Country Country { get; set; }
    }
}
