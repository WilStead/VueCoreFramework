using System.ComponentModel.DataAnnotations;

namespace VueCoreFramework.Models
{
    /// <summary>
    /// A convenience subclass of <see cref="DataItem"/> which defines a Name property and overrides ToString to display that Name.
    /// </summary>
    public class NamedDataItem : DataItem
    {
        /// <summary>
        /// The Name of the item.
        /// </summary>
        [Required]
        [Range(3, 25)]
        public string Name { get; set; }

        /// <summary>
        /// Displays the item's Name.
        /// </summary>
        public override string ToString() => Name;
    }
}
