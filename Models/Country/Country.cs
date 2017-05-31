using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    [MenuClass(IconClass = "public", Category = "Country Data")]
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
        public Guid? CityId { get; set; }

        [Display(Name = "Capitol")]
        public City City { get; set; }
    }
}
