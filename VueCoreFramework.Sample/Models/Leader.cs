using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using VueCoreFramework.Core.Data.Attributes;
using VueCoreFramework.Core.Models;

namespace VueCoreFramework.Sample.Models
{
    /// <summary>
    /// Types of marital status.
    /// </summary>
    /// <remarks>
    /// A non-Flags enum is represented by the framework as a select input, allowing a single
    /// selection. A 'None' entry, if present, will be treated the same as any other selection. The
    /// selection control will not supply a 'None' or other unset value if the enum does not contain
    /// one. The framework also will not add an unset selection placeholder for Nullable Enum
    /// properties, in order to avoid duplicating any existing 'None' entry. For this reason Nullable
    /// Enum properties are not feasible with the SPA framework (they will not cause any errors, but
    /// it will not be possible to preserve nulls; updates from the SPA will always contain a value
    /// for the property). Use a 'None' value in your enum if you wish an unset value to be
    /// represented in your data, instead of making the property nullable.
    /// </remarks>
    public enum MaritalStatus
    {
#pragma warning disable CS1591
        Married,
        Single
#pragma warning restore CS1591
    }

    /// <summary>
    /// A <see cref="NamedDataItem"/> representing a country's head of government.
    /// </summary>
    [DataClass(IconClass = "person")]
    public class Leader : NamedDataItem
    {
        /// <summary>
        /// The age of the leader.
        /// </summary>
        /// <remarks>
        /// In a real application this would surely be computed from birthdate; it is provided here
        /// purely for the demonstration of numeric input fields.
        /// </remarks>
        [Range(17, 150)]
        public int Age { get; set; }

        /// <summary>
        /// The birthdate of the leader.
        /// </summary>
        /// <remarks>
        /// A DataType of Date is represented in forms by a date picker. The time portion is ignored.
        /// Because this is a nullable property, the field will also have the option to set a null
        /// value rather than a date.
        /// </remarks>
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1/1/1917 12:00:00 AM", "1/1/2000 12:00:00 AM")]
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// The country represented by this leader.
        /// </summary>
        [JsonIgnore]
        [Hidden]
        public Country Country { get; set; }

        /// <summary>
        /// Foreign key for <see cref="Country"/>
        /// </summary>
        public Guid CountryId { get; set; }

        /// <summary>
        /// The marital status of the leader.
        /// </summary>
        [Display(Prompt = "Marital Status")]
        public MaritalStatus MaritalStatus { get; set; }

        /// <summary>
        /// Time in office (as a Ticks value).
        /// </summary>
        /// <remarks>
        /// Although the SPA framework can handle TimeSpan properties, SQL databases cannot store
        /// TimeSpans >= 24 hours, so the SPA framework can also interpret long values as TimeSpan
        /// Ticks by indicating the Duration DataType. Display Description is used to set hint text
        /// for the field. The DisplayFormat DataFormatString can be used to indicate which values of
        /// the TimeSpan will be displayed (and available for editing); it must take the form
        /// 'y:M:d:h:m:s' representing years, months, days, hours, minutes, seconds (including
        /// fractional seconds). Any number of indicators may be omitted from the beginning or end
        /// (e.g. the format below shows only years, months, and days; not hours, minutes, or
        /// seconds), but omitting any in the middle is disregarded (since overflow from a low unit
        /// must be shown and editable with in-between units). Note that Range for a Duration uses
        /// TimeSpan units, even when the property is a long.
        /// </remarks>
        [DataType(DataType.Duration)]
        [Display(Name = "Time in Office", Description = "As of 6/8/2017")]
        [DisplayFormat(DataFormatString = "y:M:d")]
        [Range(typeof(TimeSpan), "00:00:00", "36500.00:00:00")]
        public long? TimeInOfficeTicks { get; set; }
    }
}
