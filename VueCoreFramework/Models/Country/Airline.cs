using VueCoreFramework.Data.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace VueCoreFramework.Models
{
    /// <summary>
    /// A <see cref="DataItem"/> representing an airline.
    /// </summary>
    /// <remarks>
    /// Note that Airlines are being nested within the Country category, although they are both
    /// MenuClass types, and in fact the Airline type is a peer of Country, which can be created and
    /// deleted independently of any Countries. MenuClass types can be organized in the framework
    /// menu in any order and hierarchy, regardless of the database relationships they share (if
    /// any).
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

        /// <summary>
        /// Indicates whether the airline offers international flights.
        /// </summary>
        /// <remarks>
        /// Obviously this would be a caluclated property in a real application. It is implemented in
        /// this way for the purpose of demonstrating nullable boolean fields. The Description
        /// property of the Display Attribute is used to set help text.
        /// </remarks>
        [Display(Description = "The indeterminate state indicates 'unknown' for this airline.")]
        public bool? International { get; set; }
    }

    /// <summary>
    /// The many-to-many join table entity class which joins Airlines to Countries.
    /// </summary>
    /// <remarks>
    /// The SPA framework is able to recognize and interpret many-to-many join tables, but only when
    /// their only properties are a pair of navigation properties and foreign keys. A many-to-many
    /// join table with additional properties of its own will not be recognized by the SPA framework
    /// as a join table, but as an entity class unto itself, and will model each navigation as a
    /// one-to-many relationship from that class. This is likely to be the desired behavior in most
    /// cases anyhow, since additional properties will usually mean that such classes should be
    /// visible and editable on tables and forms of their own, not handled 'invisibly' as join tables.
    /// </remarks>
    public class AirlineCountry
    {
        /// <summary>
        /// The <see cref="Models.Airline"/> referred to by this relationship.
        /// </summary>
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
