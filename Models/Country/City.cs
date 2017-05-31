using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    [ChildClass(Category = "Country Data/Country")]
    public class City : DataItem
    {
        [Display(Prompt = "Name")]
        [Required]
        [Range(3, 25)]
        public string Name { get; set; }

        [Display(Name = "Population")]
        [Range(0, int.MaxValue)]
        public int Population { get; set; }

        public override string ToString() => Name;
    }
}
