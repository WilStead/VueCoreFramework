using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using VueCoreFramework.Core.Data.Attributes;

namespace VueCoreFramework.Core.Models
{
    /// <summary>
    /// A convenience subclass of <see cref="DataItem"/> which defines a name property and overrides
    /// ToString to display that name.
    /// </summary>
    public class NamedDataItem : DataItem, ICulturalDataItem
    {
        /// <summary>
        /// The name of the item.
        /// </summary>
        [Required]
        [Range(3, 25)]
        [Name]
        [Cultural]
        public string Name { get; set; }

        /// <summary>
        /// Returns the item's name for the default culture, as a <see cref="string"/>.
        /// </summary>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return null;
            }
            JObject name = JObject.Parse(Name);
            var def = (string)name.SelectToken("default");
            return string.IsNullOrEmpty(def) ? null : (string)name.SelectToken(def);
        }

        /// <summary>
        /// Returns the item's name for the given culture, as a <see cref="string"/>.
        /// </summary>
        /// <param name="culture">A culture name.</param>
        /// <returns>The item's name for the given culture, as a <see cref="string"/>.</returns>
        public string ToString(string culture)
        {
            if (string.IsNullOrEmpty(Name))
            {
                return null;
            }
            JObject name = JObject.Parse(Name);
            var value = (string)name.SelectToken(culture);
            return string.IsNullOrEmpty(value) ? ToString() : value;
        }
    }
}
