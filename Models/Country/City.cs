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
    /// as children of another object (a <see cref="Models.Country"/>, in this case).
    /// </remarks>
    [DataClass(IconClass = "location_city")]
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
        /// table, no selection will be displayed as '[None]', a selection with a valid name or
        /// description will be displayed, and any other selection (e.g. a combination of Flags
        /// values) will be displayed as '[...]', to avoid the expensive operations required to
        /// validate and break down the flag value. For this reason, it may be best to hide Flags
        /// enums in tables when it is expected that multiple selections will be common, as is done
        /// here, to avoid a column full of unhelpful placeholder text.
        /// </remarks>
        [Hidden(false, HideInTable = true)]
        public CityTransit Transit { get; set; }

        /// <summary>
        /// The country to which this city belongs.
        /// </summary>
        /// <remarks>
        /// Inverse navigation properties must be marked virtual for the framework to operate
        /// correctly. It is not necessary to hide inverse references on child objects (a field with
        /// a view/edit option will be generated). However, it will make your views cleaner when the
        /// child object has just one parent, since the parent object can always be accessed from the
        /// child by going back or cancelling the current operation, making a reverse navigation
        /// field redundant. In cases where a child may be in relationships to different parents,
        /// visible reverse navigation fields can be helpful.
        /// </remarks>
        [JsonIgnore]
        [InverseProperty(nameof(Models.Country.Cities))]
        [Hidden]
        public virtual Country Country { get; set; }

        /// <summary>
        /// The foreign key for <see cref="Country"/>.
        /// </summary>
        /// <remarks>
        /// Although Entity Framework can automatically create foreign keys, the SPA framework
        /// requires explicitly defined foreign keys for one-to-one and many-to-one relationships,
        /// which must fit the pattern {navigation property name}+'Id'. Guid properties are never
        /// shown by the framework, so it isn't necessary to mark it as hidden.
        /// </remarks>
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
        /// <remarks>
        /// Since this key is nullable, the relationship is zero-or-one-to-one, rather than
        /// one-to-one (i.e., not required).
        /// </remarks>
        public Guid? CountryCapitolId { get; set; }
    }
}
