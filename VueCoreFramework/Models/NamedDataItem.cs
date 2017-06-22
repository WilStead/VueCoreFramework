using System.ComponentModel.DataAnnotations;

namespace VueCoreFramework.Models
{
    /// <summary>
    /// A convenience subclass of <see cref="DataItem"/> which defines a name property and overrides
    /// ToString to display that name.
    /// </summary>
    public class NamedDataItem : DataItem
    {
        /// <summary>
        /// The name of the item.
        /// </summary>
        /// <remarks>
        /// Properties with the custom DataType 'Name' are treated specially by the SPA framework in
        /// a few ways: they are pulled to the left and left-aligned in data tables, and pulled to
        /// the top of forms, making them into a sort of automatic header; they also get ' (Copy)'
        /// appended during a duplication (wheras other properties are copied as-is).
        /// </remarks>
        [Required]
        [Range(3, 25)]
        [DataType("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Displays the item's name.
        /// </summary>
        public override string ToString() => Name;
    }
}
