using System.Collections.Generic;

namespace MVCCoreVue.Data
{
    public class FieldDefinition
    {
        public object Default { get; set; }
        public bool? Disabled { get; set; }
        public string GroupName { get; set; }
        public string Help { get; set; }
        public bool? HideInTable { get; set; }
        public string Hint { get; set; }
        public string Icon { get; set; }
        public string InputType { get; set; }
        public string Label { get; set; }
        public object Max { get; set; }
        public object Min { get; set; }
        public string Model { get; set; }
        public string Pattern { get; set; }
        public string Placeholder { get; set; }
        public string Prefix { get; set; }
        public bool? Readonly { get; set; }
        public bool? Required { get; set; }
        public int? Rows { get; set; }
        public double? Step { get; set; }
        public string Suffix { get; set; }
        public string Type { get; set; }
        public string Validator { get; set; }
        public List<ChoiceOption> Values { get; set; }
        public bool? Visible { get; set; }
    }

    public class ChoiceOption
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }
}
