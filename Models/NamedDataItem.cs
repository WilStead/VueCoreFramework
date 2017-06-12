using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    public class NamedDataItem : DataItem
    {
        [Required]
        [Range(3, 25)]
        public string Name { get; set; }

        public override string ToString() => Name;
    }
}
