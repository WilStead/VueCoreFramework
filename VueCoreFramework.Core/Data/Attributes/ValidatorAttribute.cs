using System;

namespace VueCoreFramework.Core.Data.Attributes
{
    /// <summary>
    /// Allows setting the validator to be used for this field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidatorAttribute : Attribute
    {
        /// <summary>
        /// The name of the validator to be used for this field. Must be a recognized name.
        /// </summary>
        /// <remarks>
        /// The validators object in vfg/vfg-custom-validators.ts is a map of names to validators (or
        /// known default validator names). The string set here must match a key in that object.
        /// </remarks>
        public string Validator { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ValidatorAttribute"/>.
        /// </summary>
        /// <param name="validator">The name of the validator to be used for this field.</param>
        public ValidatorAttribute(string validator)
        {
            Validator = validator;
        }
    }
}