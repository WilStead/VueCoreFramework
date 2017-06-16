using MVCCoreVue.Data.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MVCCoreVue.Models
{
    /// <summary>
    /// A <see cref="DataItem"/> representing an airline.
    /// </summary>
    /// <remarks>
    /// Note that Airlines are being nested within the Country category, although they are both
    /// MenuClass types, and in fact the Airline type is a peer of Country, which can be created and
    /// deleted independently of any Countries. MenuClass types can be organized in the framework
    /// menu in any order and hierarchy, regardless of the database relationships they share (if any).
    /// </remarks>
    [MenuClass(Category = "Country", IconClass = "airplanemode_active")]
    public class Airline : NamedDataItem
    {
        /// <summary>
        /// The countries in which the airline operates.
        /// </summary>
        /// <remarks>
        /// It is not necessary to hide either side of a many-to-many relationship. Here, airlines
        /// have been made conceptual children of countries: nested in their menu and accessible from
        /// their form, but not vice-versa. This is an example of a conceptual hierarchy being
        /// created in the framework to simplify the user experience, which does not represent the
        /// data structure exactly.
        /// </remarks>
        [JsonIgnore]
        [Hidden]
        public ICollection<AirlineCountry> Countries { get; set; } = new Collection<AirlineCountry>();
    }

    /// <summary>
    /// The many-to-many join table entity class which joins Airlines to Countries.
    /// </summary>
    public class AirlineCountry : IDataItemMtM
    {
        /// <summary>
        /// The <see cref="Models.Airline"/> referred to by this relationship.
        /// </summary>
        /// <remarks>
        /// The navigation properties of an <see cref="IDataItemMtM"/> object *must* be named exactly
        /// the same as the navigation properties of the entities they join. This allows the
        /// framework to find the correct properties in the relationship. Singularized forms are
        /// allowed, which is why the property is called 'Airline' rather than 'Airlines' despite the
        /// navigation property on Country being plural.
        /// </remarks>
        public Airline Airline { get; set; }

        /// <summary>
        /// The foreign key for <see cref="Airline"/>.
        /// </summary>
        public Guid AirlineId { get; set; }

        /// <summary>
        /// The <see cref="Models.Country"/> referred to by this relationship.
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        /// The foreign key for <see cref="Country"/>.
        /// </summary>
        public Guid CountryId { get; set; }
    }
}
