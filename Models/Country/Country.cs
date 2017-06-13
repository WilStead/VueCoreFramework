using MVCCoreVue.Data.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MVCCoreVue.Models
{
    [MenuClass(IconClass = "public")]
    public class Country : NamedDataItem
    {
        /// <summary>
        /// The airlines working in the country.
        /// </summary>
        /// <remarks>
        /// A many-to-many relationship with another MenuClass entity. All relationships with objects
        /// displayed on the menu must be many-to-many or many-to-one, since MenuClass objects can be
        /// added and removed independently of any relationships they may have. AirlineCountry is a
        /// many-to-many join table entity class, and therefore implenents <see
        /// cref="IDataItemMtM"/>. It must have the JsonIgnore Attribute to prevent the model from
        /// attempting to set its value to the placeholder text used in the viewmodel. It cannot be
        /// Required, since a MenuClass object must be able to be deleted independently.
        /// </remarks>
        [JsonIgnore]
        public ICollection<AirlineCountry> Airlines { get; set; } = new Collection<AirlineCountry>();

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
        /// colors). Here the field is hidden from tables, but that is not required.
        /// </remarks>
        [Display(Name = "Primary flag color")]
        [DataType("Color")]
        [Hidden(false, HideInTable = true)]
        public string FlagPrimaryColor { get; set; }

        public Guid? CapitolId { get; set; }

        [JsonIgnore]
        public City Capitol { get; set; }

        [JsonIgnore]
        public ICollection<City> Cities { get; set; } = new Collection<City>();

        /// <summary>
        /// The country's head of government.
        /// </summary>
        /// <remarks>
        /// A one-to-one relationship with a child object (an object which doesn't get listed on the
        /// menu, and is only accessible) through this parent object. It must have the JsonIgnore
        /// Attribute to prevent the model from attempting to set its value to the placeholder text
        /// used in the viewmodel. Required is optional.
        /// </remarks>
        [Required]
        [JsonIgnore]
        public Leader Leader { get; set; }

        /// <summary>
        /// The foreign key for Leader.
        /// </summary>
        /// <remarks>
        /// Although Entity Framework can automatically infer foreign keys, the SPA framework
        /// requires explicitly defined foreign keys for one-to-one relationships, which must fit the
        /// pattern {navigation property name}+'Id'. Guid properties are never shown by the
        /// framework, so it isn't required to mark it as hidden in any way.
        /// </remarks>
        [ForeignKey(nameof(Leader))]
        public Guid LeaderId { get; set; }
    }
}
