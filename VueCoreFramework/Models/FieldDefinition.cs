using System.Collections.Generic;

namespace VueCoreFramework.Models
{
    /// <summary>
    /// A ViewModel for field definitions used by vue-form-generator.
    /// </summary>
    public class FieldDefinition
    {
        /// <summary>
        /// Indicates whether the field is disabled.
        /// </summary>
        public bool? Disabled { get; set; }

        /// <summary>
        /// Optionally designates a group to which the field belongs.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Optional help text displayed with the field.
        /// </summary>
        public string Help { get; set; }

        /// <summary>
        /// Indicates that the field is hidden in data tables (but may still be visible in forms).
        /// </summary>
        public bool? HideInTable { get; set; }

        /// <summary>
        /// Optional hint text displayed with the field.
        /// </summary>
        public string Hint { get; set; }

        /// <summary>
        /// The name of a Material Icon displayed with certain fields.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The input type of certain fields which can display more than one type of data.
        /// </summary>
        public string InputType { get; set; }

        /// <summary>
        /// For navigation properties, indicates the name of the inverse navigation property.
        /// </summary>
        public string InverseType { get; set; }

        /// <summary>
        /// Indicates that the field is a Name property.
        /// </summary>
        public bool? IsName { get; set; }

        /// <summary>
        /// Optional label text displayed with the field.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// An optional maximum value for the field.
        /// </summary>
        public object Max { get; set; }

        /// <summary>
        /// An optional minimum value for the field.
        /// </summary>
        public object Min { get; set; }

        /// <summary>
        /// The name of the model property represented by the field.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// An optional regex pattern used to validate the field.
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Optional placeholder text displayed with the field.
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// Optional text displayed before a text field.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Indicates that the text field is read-only.
        /// </summary>
        public bool? Readonly { get; set; }

        /// <summary>
        /// Indicates that the field is required.
        /// </summary>
        public bool? Required { get; set; }

        /// <summary>
        /// An optional value indicating the number of rows in a textarea field.
        /// </summary>
        public int? Rows { get; set; }

        /// <summary>
        /// An optional step value for numeric text fields, and TimeSpan fields.
        /// </summary>
        public double? Step { get; set; }

        /// <summary>
        /// Optional text displayed after a text field.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Indicates the type of field. Required.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// An optional name of a validator used to validate the field.
        /// </summary>
        public string Validator { get; set; }

        /// <summary>
        /// The list of options for a select or multiselect field.
        /// </summary>
        public List<ChoiceOption> Values { get; set; }

        /// <summary>
        /// Indicates whether the field is visible on data tables and forms.
        /// </summary>
        public bool? Visible { get; set; }
    }

    /// <summary>
    /// Represents an option in a select or multiselect field.
    /// </summary>
    public class ChoiceOption
    {
        /// <summary>
        /// The text displayed for the option.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The value of the option.
        /// </summary>
        public int Value { get; set; }
    }
}
