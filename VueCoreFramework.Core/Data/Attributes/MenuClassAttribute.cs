using System;

namespace VueCoreFramework.Core.Data.Attributes
{
    /// <summary>
    /// Classes with this attribute will appear in the site menu.
    /// </summary>
    /// <remarks>
    /// Classes without this attribute will not appear on the site menu, and so will only be
    /// accessible on the user interface as child objects within MenuClass objects.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class MenuClassAttribute : DataClassAttribute
    {
        /// <summary>
        /// A string with the format "supertype/type/subtype"
        /// </summary>
        /// <remarks>
        /// Used to generate menu and URL structure; doesn't need to reflect data relationships in
        /// any way. Classes can be ordered in the framework's user interface according to a
        /// conceptual scheme for ease of understanding or navigation, while the underlying data
        /// structure is arranged according to business and/or programmatic needs, and the two
        /// designs might be only loosely correlated.
        /// </remarks>
        public string Category { get; set; }
    }
}