using System;

namespace MVCCoreVue.Data.Attributes
{
    internal class DefaultAttribute: Attribute
    {
        public object Default { get; set; }

        public DefaultAttribute(object defaultValue)
        {
            Default = defaultValue;
        }
    }

    internal class HiddenAttribute : Attribute
    {
        public bool Hidden { get; set; }

        public bool HiddenInTable { get; set; }

        public HiddenAttribute(bool hidden = false)
        {
            Hidden = hidden;
        }
    }

    internal class RowsAttribute : Attribute
    {
        public int Rows { get; set; }

        public RowsAttribute(int rows)
        {
            Rows = rows;
        }
    }

    internal class StepAttribute : Attribute
    {
        public double Step { get; set; }

        public StepAttribute(double step)
        {
            Step = step;
        }
    }

    internal class ValidatorAttribute : Attribute
    {
        public string Validator { get; set; }

        public ValidatorAttribute(string validator)
        {
            Validator = validator;
        }
    }
}