using MVCCoreVue.Data.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCCoreVue.Data
{
    public class DataItem
    {
        [Editable(false)]
        [Hidden]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        
        [Display(AutoGenerateField = false)]
        public DateTime CreationTimestamp { get; set; }

        [Display(AutoGenerateField = false)]
        public DateTime UpdateTimestamp { get; set; }
    }
}
