using System;

namespace VueCoreFramework.Core.Data.Attributes
{
    /// <summary>
    /// Indicates that the property is a Name. Name properties are treated specially by the SPA
    /// framework in a few ways: they are pulled to the left and left-aligned in data tables, and
    /// pulled to the top of forms, making them into a sort of automatic header; they also get '
    /// (Copy)' appended during a duplication (whereas other properties are copied as-is).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NameAttribute : Attribute { }
}