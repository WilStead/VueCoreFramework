using System;
using System.ComponentModel;

namespace VueCoreFramework.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the description of the Enum value, or the Name of the value if no explicit description has been set.
        /// </summary>
        /// <returns>
        /// The description, or null if the value is not named (e.g. a combined Flags value, or a value which is simply incorrect).
        /// </returns>
        public static string GetDescription(Type type, object item)
        {
            string description = null;
            if (item == null)
            {
                return description;
            }
            var attr = item.GetAttribute<DescriptionAttribute>();
            if (attr == null)
            {
                try
                {
                    description = Enum.GetName(type, item);
                }
                catch
                {
                    // An exception indicates the value is not defined in the Enum.
                    // This is not treated as an error here; a combined Flags value could be the cause.
                    // The original null value is returned.
                }
            }
            else
            {
                description = attr.Description;
            }
            return description;
        }
    }
}
