using System;

namespace MVCCoreVue.Data.Attributes
{
    internal class HiddenAttribute : Attribute { }

    internal class ValidatorAttribute : Attribute
    {
        public string Validator { get; set; }

        public ValidatorAttribute(string validator)
        {
            Validator = validator;
        }
    }
}