﻿using MVCCoreVue.Data.Attributes;
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
    /// Note that airlines are being nested within the Country category, although they are both
    /// MenuClass types, and in fact the Airline class is a peer of Country which can be created and
    /// deleted independently of any Countries. DataItem classes can be organized in the framework
    /// menu in any order and hierarchy, regardless of the database relationships they share (if any).
    /// </remarks>
    [MenuClass(Category = "Country")]
    public class Airline : NamedDataItem
    {
        /// <summary>
        /// The countries in which the airline operates.
        /// </summary>
        /// <remarks>
        /// It is not necessary to hide either side of a many-to-many relationship. Here, airlines
        /// have been made conceptual children of countries: nested in their menu and accessible from
        /// their form, but not vice-versa. This is an example of a conceptual relationship being
        /// created in the framework for display purposes and ease of navigation.
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
        /// The <see cref="Airline"/> referred to by this relationship.
        /// </summary>
        /// <remarks>
        /// The navigation properties of an <see cref="IDataItemMtM"/> object *must* be named exactly
        /// the same as the navigation properties of the entities they join (change of pluralization
        /// is not allowed, which is why this property is called 'Airlines' rather than 'Airline'
        /// which might seem more natural, since each AirlineCountry object connects a single Airline
        /// and Country). This allows the framework to find the correct properties in the relationship.
        /// </remarks>
        public Airline Airlines { get; set; }

        /// <summary>
        /// The foreign key for <see cref="Airlines"/>.
        /// </summary>
        public Guid AirlinesId { get; set; }

        /// <summary>
        /// The <see cref="Country"/> referred to by this relationship.
        /// </summary>
        public Country Countries { get; set; }

        /// <summary>
        /// The foreign key for <see cref="Countries"/>.
        /// </summary>
        public Guid CountriesId { get; set; }
    }
}
