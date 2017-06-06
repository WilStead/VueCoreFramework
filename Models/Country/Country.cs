using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    [MenuClass(IconClass = "public")]
    public class Country : DataItem
    {
        [Required]
        [Range(3, 25)]
        public string Name { get; set; }

        [Display(Prompt = "EPI Index")]
        [Required]
        [Range(0, 100)]
        [Step(0.01)]
        public double EpiIndex { get; set; }

        [Display(AutoGenerateField = false)]
        public Guid CapitolId { get; set; }

        [JsonIgnore]
        public City Capitol { get; set; }

        [JsonIgnore]
        public ICollection<City> Cities { get; set; }

        [Display(AutoGenerateField = false)]
        public Guid LeaderId { get; set; }

        [Required]
        [JsonIgnore]
        public Leader Leader { get; set; }
    }
}
