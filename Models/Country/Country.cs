using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    [MenuClass(IconClass = "public")]
    public class Country : DataItem
    {
        [Display(Prompt = "Name")]
        [Required]
        [Range(3, 25)]
        public string Name { get; set; }

        [Display(Name = "EPI Index")]
        [Required]
        [Range(0, 100)]
        public double EpiIndex { get; set; }

        [Display(AutoGenerateField = false)]
        public Guid CapitolId { get; set; }

        public City Capitol { get; set; }
        
        public ICollection<City> Cities { get; set; }

        [Display(AutoGenerateField = false)]
        public Guid LeaderId { get; set; }

        [Required]
        public Leader Leader { get; set; }
    }
}
