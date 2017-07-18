using System;

namespace VueCoreFramework.Core.Data.Attributes
{
    /// <summary>
    /// Allows setting an icon for text, checkbox, and select fields. Other fields will ignore this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IconAttribute : Attribute
    {
        /// <summary>
        /// The name of a Material Icon which will decorate the field.
        /// </summary>
        /// <remarks>Spaces should be replaced by underscores in icon names.</remarks>
        public string Icon { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="IconAttribute"/>.
        /// </summary>
        /// <param name="icon">The name of a Material Icon which will decorate the field.</param>
        public IconAttribute(string icon) { Icon = icon; }
    }
}