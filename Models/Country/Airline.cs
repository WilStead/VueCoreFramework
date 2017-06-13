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
        [Hidden]
        public ICollection<AirlineCountry> Countries { get; set; } = new Collection<AirlineCountry>();
    }

    public class AirlineCountry : IDataItemMtM
    {
        public Guid AirlinesId { get; set; }
        public Airline Airlines { get; set; }

        public Guid CountriesId { get; set; }
        public Country Countries { get; set; }
    }
}
