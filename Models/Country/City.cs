using MVCCoreVue.Data;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    public class City : DataItem
    {
        [Display(Prompt = "Name")]
        [Required]
        [Range(3, 25)]
        public string Name { get; set; }

        [Display(Name = "Population")]
        [Range(0, 100)]
        public int Population { get; set; }

        public override string ToString() => Name;
    }
}
