using System;

namespace VueCoreFramework.Core.Data.Attributes
{
    /// <summary>
    /// Allows marking a property as hidden in the user interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HiddenAttribute : Attribute
    {
        /// <summary>
        /// Indicates that the field is not displayed in the user interface at all.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Indicates that the field is not displayed in data tables.
        /// </summary>
        /// <remarks>
        /// If this property is true but <see cref="Hidden"/> is false, the property will be
        /// displayed (and is possibly editable) in forms, but is not listed in data tables only.
        /// This can be useful for keeping table listings brief, or for hiding entries with rich edit
        /// controls but ugly plain text formats (shown in data tables).
        /// </remarks>
        public bool HideInTable { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="HiddenAttribute"/>.
        /// </summary>
        /// <param name="hidden">Indicates that the property is not displayed in the user interface at all.</param>
        public HiddenAttribute(bool hidden = true)
        {
            Hidden = hidden;
        }
    }
}