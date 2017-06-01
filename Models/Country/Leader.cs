using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVCCoreVue.Models
{
    [ChildClass(Category = "Country")]
    public class Leader : DataItem
    {
        [Display(Prompt = "Name")]
        [Required]
        [Range(3, 25)]
        public string Name { get; set; }

        [Range(0, 150)]
        public int Age { get; set; }

        public override string ToString() => Name;
    }
}
