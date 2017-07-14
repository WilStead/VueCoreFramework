using VueCoreFramework.Data.Attributes;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VueCoreFramework.Models
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
#pragma warning disable CS1591
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
#pragma warning restore CS1591
    }

    /// <summary>
    /// A <see cref="NamedDataItem"/> representing a city.
    /// </summary>
    /// <remarks>
    /// Because this class doesn't have the <see cref="MenuClassAttribute"/>, it will not appear in
    /// the menu of the SPA, and items of this type will therefore only be available to view or edit
    /// as children of another object (a <see cref="Models.Country"/>, in this case). The
    /// DashboardFormContent property indicates that a custom component exists which will be
    /// displayed above data forms when viewing cities.
    /// </remarks>
    [DataClass(DashboardFormContent = "country/city", IconClass = "location_city")]
    public class City : NamedDataItem
    {
        /// <summary>
        /// The local time in the city at GMT midnight.
        /// </summary>
        /// <remarks>
        /// The DataType can control the type of field displayed in forms. Time will present a time
        /// picker alone. Without this specification, a DateTime property would be represented by
        /// both date and timne pickers.
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
        /// here, to avoid a column full of unhelpful placeholder text. When single selections are
        /// expected to be more common, the field can be left visible so that the named selections
        /// can be shown in tables.
        /// </remarks>
        [Hidden(false, HideInTable = true)]
        public CityTransit Transit { get; set; }

        /// <summary>
        /// The country to which this city belongs.
        /// </summary>
        /// <remarks>
        /// It is not necessary to hide inverse references on child objects (a field with a view/edit
        /// option will be generated). However, it can make views cleaner when the child object has
        /// just one parent, since the parent object can always be accessed from the child by going
        /// back or cancelling the current operation, making a reverse navigation field redundant. In
        /// cases where a child may be in relationships to different parents, visible reverse
        /// navigation fields can be helpful to browse among the related entities.
        /// </remarks>
        [JsonIgnore]
        [InverseProperty(nameof(Models.Country.Cities))]
        [Hidden]
        public Country Country { get; set; }

        /// <summary>
        /// The foreign key for <see cref="Country"/>.
        /// </summary>
        /// <remarks>
        /// Although Entity Framework can automatically create foreign keys, the SPA framework
        /// requires explicitly defined foreign keys for one-to-one and many-to-one relationships.
        /// Foreign key properties are never shown by the framework, so it isn't necessary to mark it
        /// as hidden.
        /// </remarks>
        public Guid CountryId { get; set; }

        /// <summary>
        /// Indicates that this city is the capitol of the country.
        /// </summary>
        /// <remarks>
        /// Since only one city may be the capitol, but there is no way to enforce such a constraint
        /// with the Entity Framework and VueCoreFramework Attribute decoration system, a custom
        /// control is provided on the City form to perform this function instead. Therefore, it is
        /// hidden from the normal form (but not in tables).
        /// </remarks>
        [Hidden(true, HideInTable = false)]
        public bool IsCapitol { get; set; }
    }
}
