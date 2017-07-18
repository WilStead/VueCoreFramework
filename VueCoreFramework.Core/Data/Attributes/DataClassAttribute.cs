using System;

namespace VueCoreFramework.Core.Data.Attributes
{
    /// <summary>
    /// Sets data type-specific information for use in the SPA framework.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DataClassAttribute : Attribute
    {
        /// <summary>
        /// The name of a Material Icon, will appear as the class's icon in the dashboard.
        /// </summary>
        /// <remarks>Spaces should be replaced by underscores in icon names.</remarks>
        public string IconClass { get; set; }

        /// <summary>
        /// Indicates that the IconClass is a FontAwesome icon, rather than a Material Icon.
        /// </summary>
        public bool FontAwesome { get; set; }

        /// <summary>
        /// An optional path to a Vue component which is shown on the data class' dashboard above
        /// data tables. Paths are relative to the components/data folder.
        /// </summary>
        public string DashboardTableContent { get; set; }

        /// <summary>
        /// An optional path to a Vue component which is shown on the data class' dashboard above
        /// forms. Paths are relative to the components/data folder. This component will receive the
        /// operation being performed and the id of the item as props.
        /// </summary>
        public string DashboardFormContent { get; set; }
    }
}