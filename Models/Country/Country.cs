using MVCCoreVue.Data.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCCoreVue.Models
{
    /// <summary>
    /// A <see cref="DataItem"/> representing a country.
    /// </summary>
    /// <remarks>
    /// Because this class is decorated with the MenuClass attribute, it will appear in the
    /// framework's displayed menu.
    /// </remarks>
    [MenuClass(IconClass = "public")]
    public class Country : NamedDataItem
    {
        /// <summary>
        /// The airlines operating in the country.
        /// </summary>
        /// <remarks>
        /// A many-to-many relationship with another MenuClass entity. All relationships with objects
        /// displayed on the menu must be many-to-many or many-to-one, since MenuClass objects can be
        /// added and removed independently of any relationships they may have. Furthermore, only
        /// MenuClass types should be in many-to-many relationships, because if a child type is
        /// removed from all parents before being deleted, it will be left inaccessible.
        /// AirlineCountry is a many-to-many join table entity class, and therefore implenents <see
        /// cref="IDataItemMtM"/>. It must have the JsonIgnore Attribute to prevent the model from
        /// attempting to set its value to the placeholder text used in the viewmodel. It cannot be
        /// Required, since a MenuClass object must be able to be deleted independently. It has a
        /// property constructor in order to avoid a null collection during certain framework
        /// operations; this is a requirement for all collection properties of a DataItem.
        /// </remarks>
        [JsonIgnore]
        public ICollection<AirlineCountry> Airlines { get; set; } = new Collection<AirlineCountry>();

        /// <summary>
        /// The EPI Index of the country.
        /// </summary>
        /// <remarks>
        /// The Display Attribute is used to control the decoration of the field. Prompt will set
        /// placeholder text for most fields. GroupName can be used to group fields together under a
        /// heading. Range can be used for numeric, text, DateTime, or TimeSpan fields. For text
        /// fields, it indicates required length.
        /// </remarks>
        [Display(Prompt = "EPI Index")]
        [Range(0, 100)]
        [Step(0.01)]
        public double EpiIndex { get; set; }

        /// <summary>
        /// The main color of the country's flag.
        /// </summary>
        /// <remarks>
        /// The 'Color' custom DataType is special in the framework, and will generate a color picker
        /// control in edit forms. The data is represented as a 7-character hex string (#000000), and
        /// this raw value is shown on tables (which only display text, not rich content like
        /// colors). Here the field is hidden from tables, but that is not required. Display Name is
        /// used to set a label for the input group (color fields do not use placeholder text).
        /// </remarks>
        [Display(Name = "Primary flag color")]
        [DataType("Color")]
        [Hidden(false, HideInTable = true)]
        public string FlagPrimaryColor { get; set; }

        /// <summary>
        /// The country's capitol city.
        /// </summary>
        /// <remarks>
        /// A one-to-one relationship with a child object (an object which doesn't get listed on the
        /// menu, and is only accessible through this parent object). The framework requires that all
        /// relationships except many-to-many relationships specify the InverseProperty explicitly on
        /// both ends (even when EntityFramework does not), in order to identify the correct
        /// properties in the relationship.
        /// </remarks>
        [JsonIgnore]
        [InverseProperty(nameof(City.CountryCapitol))]
        public City Capitol { get; set; }

        /// <summary>
        /// The cities of the country.
        /// </summary>
        /// <remarks>
        /// A many-to-one relationship with a child object.
        /// </remarks>
        [JsonIgnore]
        [InverseProperty(nameof(City.Country))]
        public ICollection<City> Cities { get; set; } = new Collection<City>();

        /// <summary>
        /// The country's head of government.
        /// </summary>
        /// <remarks>
        /// A one-to-one relationship with a child object. Unlike Capitol, this one is required.
        /// It is necessary to explicitly mark required child objects with the Required Attribute
        /// so that the default constructor will know to create a new empty object.
        /// </remarks>
        [Required]
        [JsonIgnore]
        [InverseProperty(nameof(Models.Leader.Country))]
        public Leader Leader { get; set; }
    }
}
