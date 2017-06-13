using MVCCoreVue.Data.Attributes;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCCoreVue.Models
{
    /// <summary>
    /// Types of marital status.
    /// </summary>
    /// <remarks>
    /// A non-Flags enum is represented by the framework as a select input, allowing a single
    /// selection. A 'None' value, if present, will be treated the same as any other selection.
    /// </remarks>
    public enum MaritalStatus
    {
        Married,
        Single
    }

    /// <summary>
    /// A <see cref="DataItem"/> representing a country's head of government.
    /// </summary>
    public class Leader : NamedDataItem
    {
        /// <summary>
        /// The age of the leader.
        /// </summary>
        /// <remarks>
        /// In a real application this would surely be computed from birthdate; it is provided here
        /// purely as for the demonstration of numeric input fields.
        /// </remarks>
        [Range(17, 150)]
        public int Age { get; set; }

        /// <summary>
        /// The birthdate of the leader.
        /// </summary>
        /// <remarks>
        /// A DataType of Date is represented in forms by a date picker. The time portion is ignored.
        /// </remarks>
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1/1/1917 12:00:00 AM", "1/1/2000 12:00:00 AM")]
        public DateTime Birthdate { get; set; }

        /// <summary>
        /// The country represented by this leader.
        /// </summary>
        [JsonIgnore]
        [Required]
        [Hidden]
        [InverseProperty(nameof(Models.Country.Leader))]
        public virtual Country Country { get; set; }

        /// <summary>
        /// Foreign key for <see cref="Country"/>
        /// </summary>
        /// <remarks>
        /// Since this relationship is required (one-to-one), the key is not nullable.
        /// </remarks>
        public Guid CountryId { get; set; }

        /// <summary>
        /// The marital status of the leader.
        /// </summary>
        /// <remarks>
        /// Mullable Enum properties are treated exactly the same as required ones by the framework,
        /// since the selection controls have no way to indicate an unset value. Use a 'None' value
        /// in your enum if you wish an unset value to be represented in your data.
        /// </remarks>
        [Display(Prompt = "Marital Status")]
        public MaritalStatus MaritalStatus { get; set; }

        /// <summary>
        /// Time in office.
        /// </summary>
        /// <remarks>
        /// Durations can be represented on forms with TimeSpan properties, but SQL databases cannot
        /// store TimeSpans greater than 7 days. Instead, a computed TimeSpan property based on the
        /// long Ticks property above is used with a long Ticks property as the actual
        /// database-mapped property. Display Description is used set hint text for the field. The
        /// DataFormatString property of DisplayFormat can be used to indicate which values of the
        /// TimeSpan should be displayed (and available for editing); it must take the form
        /// 'y:M:d:h:m:s' representing years, months, days, hours, minutes, seconds (including
        /// fractional seconds). Any number of indicators may be omitted from the beginning or end
        /// (e.g. the format below shows only years, months, and days; not hours, minutes, or
        /// seconds), but omitting them from the middle is disregarded (since overflow from a low
        /// unit must be shown and editable with in-between units).
        /// </remarks>
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

        /// <summary>
        /// Time in office as a ticks value.
        /// </summary>
        /// <remarks>
        /// It is not necessary for this 'pseudo-backing-store' to be hidden, but it is recommended,
        /// since users will not likely want to see a raw Ticks value.
        /// </remarks>
        [Hidden]
        public long TimeInOfficeTicks { get; set; }
    }
}
