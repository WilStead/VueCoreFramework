using MVCCoreVue.Data;
using MVCCoreVue.Data.Attributes;
using System;
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

        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1/1/1917 12:00:00 AM", "1/1/2000 12:00:00 AM")]
        public DateTime Birthdate { get; set; }

        [Display(Prompt = "Time in Office")]
        [DataType(DataType.Duration)]
        [Range(typeof(TimeSpan), "00:00:00", "36500.00:00:00")]
        public TimeSpan TimeInOffice { get; set; }

        [Display(Prompt = "Marital Status")]
        public MaritalStatus MaritalStatus { get; set; }

        public override string ToString() => Name;
    }
}
