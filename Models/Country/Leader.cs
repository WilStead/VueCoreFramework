using MVCCoreVue.Data.Attributes;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCCoreVue.Models
{
    public enum MaritalStatus
    {
        Married,
        Single
    }

    public class Leader : NamedDataItem
    {
        [Range(17, 150)]
        public int Age { get; set; }

        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1/1/1917 12:00:00 AM", "1/1/2000 12:00:00 AM")]
        public DateTime Birthdate { get; set; }

        [Hidden]
        public long TimeInOfficeTicks { get; set; }

        [NotMapped]
        [Display(Name = "Time in Office", Description = "As of 6/8/2017")]
        [DataType(DataType.Duration)]
        [DisplayFormat(DataFormatString = "y:M:d")]
        [Range(typeof(TimeSpan), "00:00:00", "36500.00:00:00")]
        public TimeSpan TimeInOffice
        {
            get => TimeSpan.FromTicks(TimeInOfficeTicks);
            set => TimeInOfficeTicks = value.Ticks;
        }

        [Display(Prompt = "Marital Status")]
        public MaritalStatus MaritalStatus { get; set; }

        [ForeignKey(nameof(Country))]
        public Guid LeaderCountryId { get; set; }

        [JsonIgnore]
        [Required]
        [Hidden]
        public virtual Country Country { get; set; }
    }
}
