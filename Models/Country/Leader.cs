using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    [ChildClass(Category = "Country")]
    public class Leader : DataItem
    {
        [Required]
        [Range(3, 25)]
        public string Name { get; set; }

        [Range(0, 150)]
        public int Age { get; set; }

        public override string ToString() => Name;
    }
}
