using System;

namespace VueCoreFramework.Core.Data.Attributes
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
    public class CulturalAttribute : Attribute { }
}