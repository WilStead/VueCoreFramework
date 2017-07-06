using System;

namespace VueCoreFramework.Data.Attributes
{
    /// <summary>
    /// Indicates that this property provides culture-specific values.
    /// </summary>
    /// <remarks>
    /// Only text properties are valid targets for this Attribute. The property type must be <see
    /// cref="string"/>, as they are stored as JSON objects with culture codes (e.g. "en-US") as
    /// keys, as well as a key called "default" whose value indicates the default culture of the
    /// object. The entire JSON object is sent with the ViewModel for the object, but the SPA
    /// framework will only display the value corresponding to the culture set for the current user.
    /// If the current culture is not available in the JSON object, the value for the "default"
    /// culture will be displayed. When setting a new value, the SPA framework will set the value for
    /// the current culture. If no other values have been set, it will also set the "default" value
    /// to the current culture.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    internal class CulturalAttribute : Attribute { }

    /// <summary>
    /// Sets data type-specific information for use in the SPA framework.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class DataClassAttribute : Attribute
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

    /// <summary>
    /// Allows setting help text for a property's field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class HelpAttribute : Attribute
    {
        /// <summary>
        /// The help text for the field.
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="helpText">The help text for the field.</param>
        public HelpAttribute(string helpText)
        {
            HelpText = helpText;
        }
    }

    /// <summary>
    /// Allows marking a property as hidden in the user interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class HiddenAttribute : Attribute
    {
        /// <summary>
        /// Indicates that the field is not displayed in the user interface at all.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Indicates that the field is not displayed in data tables.
        /// </summary>
        /// <remarks>
        /// If this property is true but <see cref="Hidden"/> is false, the property will be
        /// displayed (and is possibly editable) in forms, but is not listed in data tables only.
        /// This can be useful for keeping table listings brief, or for hiding entries with rich edit
        /// controls but ugly plain text formats (shown in data tables).
        /// </remarks>
        public bool HideInTable { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="hidden">Indicates that the property is not displayed in the user interface at all.</param>
        public HiddenAttribute(bool hidden = true)
        {
            Hidden = hidden;
        }
    }

    /// <summary>
    /// Allows setting an icon for text, checkbox, and select fields. Other fields will ignore this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class IconAttribute : Attribute
    {
        /// <summary>
        /// The name of a Material Icon which will decorate the field.
        /// </summary>
        /// <remarks>Spaces should be replaced by underscores in icon names.</remarks>
        public string Icon { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="icon">The name of a Material Icon which will decorate the field.</param>
        public IconAttribute(string icon) { Icon = icon; }
    }

    /// <summary>
    /// Classes with this attribute will appear in the site menu.
    /// </summary>
    /// <remarks>
    /// Classes without this attribute will not appear on the site menu, and so will only be
    /// accessible on the user interface as child objects within MenuClass objects.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    internal class MenuClassAttribute : DataClassAttribute
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

    /// <summary>
    /// Indicates that the property is a Name. Name properties are treated specially by the SPA
    /// framework in a few ways: they are pulled to the left and left-aligned in data tables, and
    /// pulled to the top of forms, making them into a sort of automatic header; they also get '
    /// (Copy)' appended during a duplication (whereas other properties are copied as-is).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class NameAttribute : Attribute { }

    /// <summary>
    /// Allows setting the step value of a numeric field. Ignored for other field types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class StepAttribute : Attribute
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

    /// <summary>
    /// A general-purpose attribute for setting properties of a text field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class TextAttribute : Attribute
    {
        /// <summary>
        /// A suffix appended to the text field. The suffix is decorative only, not treated as part of the value.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// A prefix appended to the text field. The prefix is decorative only, not treated as part of the value.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// The number of rows in a multi-line textarea.
        /// </summary>
        /// <remarks>
        /// Setting this to a value greater than 1 will automatically generate a textarea field, even
        /// if the DataType has not been set to MultilineText. Values less than 1 are ignored.
        /// </remarks>
        public int Rows { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TextAttribute() { }
    }

    /// <summary>
    /// Allows setting the validator to be used for this field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class ValidatorAttribute : Attribute
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
        /// Default constructor.
        /// </summary>
        /// <param name="validator">The name of the validator to be used for this field.</param>
        public ValidatorAttribute(string validator)
        {
            Validator = validator;
        }
    }
}