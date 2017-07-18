using System;

namespace VueCoreFramework.Core.Data.Attributes
{
    /// <summary>
    /// Allows setting help text for a property's field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HelpAttribute : Attribute
    {
        /// <summary>
        /// The help text for the field.
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="HelpAttribute"/>.
        /// </summary>
        /// <param name="helpText">The help text for the field.</param>
        public HelpAttribute(string helpText)
        {
            HelpText = helpText;
        }
    }
}