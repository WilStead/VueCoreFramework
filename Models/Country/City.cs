using MVCCoreVue.Data.Attributes;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCCoreVue.Models
{
    /// <summary>
    /// Types of city transportation.
    /// </summary>
    /// <remarks>
    /// A Flags enum is represented by the framework as a multiselect input, allowing 0 or more
    /// selections. A 'None' value (0) is not required for the framework, and will be a functionless
    /// placeholder in the dropdown (selecting it will have no effect).
    /// </remarks>
    [Flags]
    public enum CityTransit
    {
        None = 0,
        Airport = 1,

        /// <remarks>
        /// Description can be used to set the display value used in the dropdown. The name of the
        /// value is used otherwise.
        /// </remarks>
        [Description("Bus Station")]
        BusStation = 2,

        [Description("Train Depot")]
        TrainDepot = 4
    }

    /// <summary>
    /// A <see cref="DataItem"/> representing a city.
    /// </summary>
    /// <remarks>
    /// Because this class doesn't have the <see cref="MenuClassAttribute"/>, it will not appear in
    /// the menu of the SPA, and items of this type will therefore only be available to view or edit
    /// as children of another object (a <see cref="Models.Country"/>, in this case). This is required for
    /// any class which has a one-to-one or one-to-many relationship with any MenuClass parent type,
    /// since being listed in the menu would allow an item to be created and deleted independently of
    /// its parent, and the framework would not be able to manage the necessary relationships correctly.
    /// </remarks>
    public class City : NamedDataItem
    {
        /// <summary>
        /// The local time in the city at GMT midnight.
        /// </summary>
        /// <remarks>
        /// The DataType can control the type of field displayed in forms. Time will present a time
        /// picker.
        /// </remarks>
        [Display(Prompt = "Local time at GMT midnight")]
        [DataType(DataType.Time)]
        public DateTime LocalTimeAtGMTMidnight { get; set; }

        /// <summary>
        /// The population of the city.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Population { get; set; }

        /// <summary>
        /// The types of transportation available in the city.
        /// </summary>
        /// <remarks>
        /// A Flags enum property is represented by the framework as a multiselect input. In a data
        /// table, no selection will be displayed as '[None]' and any other selection will be
        /// displayed as '[...]', to avoid the expensive operations required to validate and break
        /// down the flag value.
        /// </remarks>
        public CityTransit Transit { get; set; }

        /// <summary>
        /// The country to which this city belongs.
        /// </summary>
        /// <remarks>
        /// Inverse navigation properties on child objects must be marked virtual, to prevent
        /// deleting parent objects incorrectly. It is not necessary to hide inverse references on
        /// child objects, although it is recommended to avoid a confusing structure; the child
        /// object should usually be accessible only from the parent in the first place, which means
        /// the parent can be accessed simply by going 'back' or canceling the current operation,
        /// which makes a reverse navigation field unnecessary.
        /// </remarks>
        [JsonIgnore]
        [InverseProperty(nameof(Models.Country.Cities))]
        [Hidden]
        public virtual Country Country { get; set; }

        /// <summary>
        /// The foreign key for <see cref="Country"/>.
        /// </summary>
        public Guid CountryId { get; set; }

        /// <summary>
        /// The country of which this city is the capitol.
        /// </summary>
        [JsonIgnore]
        [InverseProperty(nameof(Models.Country.Capitol))]
        [Hidden]
        public virtual Country CountryCapitol { get; set; }

        /// <summary>
        /// The foreign key for <see cref="CountryCapitol"/>.
        /// </summary>
        public Guid CountryCapitolId { get; set; }
    }
}
