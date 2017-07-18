using System;

namespace VueCoreFramework.Core.Data.Attributes
{
    /// <summary>
    /// A general-purpose attribute for setting properties of a text field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TextAttribute : Attribute
    {
        /// <summary>
        /// A suffix appended to the text field. The suffix is decorative only, not treated as part of the value.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// A prefix appended to the text field. The prefix is decorative only, not treated as part of the value.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// The number of rows in a multi-line textarea.
        /// </summary>
        /// <remarks>
        /// Setting this to a value greater than 1 will automatically generate a textarea field, even
        /// if the DataType has not been set to MultilineText. Values less than 1 are ignored.
        /// </remarks>
        public int Rows { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="TextAttribute"/>.
        /// </summary>
        public TextAttribute() { }
    }
}