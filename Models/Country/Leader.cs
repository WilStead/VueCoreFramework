using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    public enum MaritalStatus
    {
        Married,
        Single
    }

    [ChildClass(Category = "Country")]
    public class Leader : DataItem
    {
        [Required]
        [Range(3, 25)]
        public string Name { get; set; }

        [Range(0, 150)]
        public int Age { get; set; }

        [Display(Prompt = "Marital Status")]
        public MaritalStatus MaritalStatus { get; set; }

        public override string ToString() => Name;
    }
}
