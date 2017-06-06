using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    [MenuClass(Category = "Country")]
    public class City : DataItem
    {
        [Required]
        [Range(3, 25)]
        public string Name { get; set; }

        [Range(0, int.MaxValue)]
        public int Population { get; set; }

        public override string ToString() => Name;
    }
}
