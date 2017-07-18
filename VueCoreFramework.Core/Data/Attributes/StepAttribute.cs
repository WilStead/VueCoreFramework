using System;

namespace VueCoreFramework.Core.Data.Attributes
{
    /// <summary>
    /// Allows setting the step value of a numeric field. Ignored for other field types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class StepAttribute : Attribute
    {
        /// <summary>
        /// The step value of the field.
        /// </summary>
        /// <remarks>
        /// Always treated as a positive value, even if a negative number is set.
        /// </remarks>
        public double Step { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="step">The step value of the field.</param>
        public StepAttribute(double step)
        {
            Step = step;
        }
    }
}