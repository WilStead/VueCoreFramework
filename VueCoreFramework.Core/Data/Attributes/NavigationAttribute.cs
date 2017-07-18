using System;

namespace VueCoreFramework.Core.Data.Attributes
{
    /// <summary>
    /// Allow you to manage the controls shown on the field used to display non-collection navigation properties.
    /// </summary>
    /// <remarks>
    /// This property has no effect for collection navigation properties.
    /// </remarks>
    public class NavigationAttribute : Attribute
    {
        /// <summary>
        /// Setting this property to "reference" restricts the field to displaying only
        /// view/edit controls (no add, select, or delete controls, even if the relationship would
        /// normally allow these).
        /// Setting this property to "single" restricts the field to displaying only
        /// add, delete, and view/edit controls (no select control even for many-to-one relationships).
        /// </summary>
        public string NavigationType { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NavigationAttribute"/>.
        /// </summary>
        /// <param name="type">
        /// Setting this property to "reference" restricts the field to displaying only view/edit
        /// controls (no add, select, or delete controls, even if the relationship would normally
        /// allow these). Setting this property to "single" restricts the field to displaying only
        /// add, delete, and view/edit controls (no select control even for many-to-one relationships).
        /// </param>
        public NavigationAttribute(string type = "reference")
        {
            NavigationType = type;
        }
    }
}