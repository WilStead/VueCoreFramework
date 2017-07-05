using VueCoreFramework.Data.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace VueCoreFramework.Models
{
    /// <summary>
    /// A <see cref="NamedDataItem"/> representing a country.
    /// </summary>
    /// <remarks>
    /// Because this class is decorated with the MenuClass attribute, it will appear in the
    /// SPA framework's displayed menu.
    /// </remarks>
    [MenuClass(IconClass = "public")]
    public class Country : NamedDataItem
    {
        /// <summary>
        /// The airlines operating in the country.
        /// </summary>
        /// <remarks>
        /// A many-to-many relationship with another MenuClass entity. AirlineCountry is a
        /// many-to-many join table entity class. Navigation properties must have the JsonIgnore
        /// Attribute to prevent the model from attempting to set its value to the placeholder text
        /// used in the ViewModel. It isn't necesary to hide navigation properties in data tables,
        /// but since all a table will show is '[None]' for an empty collection and '[...]' for a
        /// non-empty collection, it will usually provide a better user experience to avoid filling a
        /// column with such placeholder text. It has a property constructor in order to avoid a null
        /// collection during certain framework operations; this is a requirement of all collection
        /// properties for the SPA framework.
        /// </remarks>
        [JsonIgnore]
        [Hidden(false, HideInTable = true)]
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
        public double? EpiIndex { get; set; }

        /// <summary>
        /// The main color of the country's flag.
        /// </summary>
        /// <remarks>
        /// The 'Color' custom DataType is special in the SPA framework, and will generate a color
        /// picker control in edit forms. The data is represented as a 7-character hex string
        /// (#000000), and this raw value is shown on tables (which only display text, not rich
        /// content like colors). The field is hidden from tables here to avoid such a
        /// non-user-friendly display format, but that is not required. Display Name is used to set a
        /// label for the input group (color fields do not use placeholder text).
        /// </remarks>
        [Display(Name = "Primary flag color")]
        [DataType("Color")]
        [Hidden(false, HideInTable = true)]
        public string FlagPrimaryColor { get; set; }

        /// <summary>
        /// The country's capitol city.
        /// </summary>
        /// <remarks>
        /// This is a computed property which finds the child with the relevant property. It is
        /// marked read-only because it is used to read-only information in forms and tables.
        /// </remarks>
        [Editable(false)]
        public string Capitol => Cities.FirstOrDefault(c => c.IsCapitol)?.Name;

        /// <summary>
        /// The cities of the country.
        /// </summary>
        /// <remarks>
        /// A many-to-one relationship with a child object.
        /// </remarks>
        [JsonIgnore]
        [InverseProperty(nameof(City.Country))]
        [Hidden(false, HideInTable = true)]
        public ICollection<City> Cities { get; set; } = new Collection<City>();

        /// <summary>
        /// The country's head of government.
        /// </summary>
        /// <remarks>
        /// A one-to-one relationship with a child object.
        /// </remarks>
        [JsonIgnore]
        public Leader Leader { get; set; }
    }
}
