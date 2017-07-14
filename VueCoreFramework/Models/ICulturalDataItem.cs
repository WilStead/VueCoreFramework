namespace VueCoreFramework.Models
{
    /// <summary>
    /// Types which implement this interface provide an overload of <see cref="ToString"/> which
    /// accepts a culture.
    /// </summary>
    public interface ICulturalDataItem
    {
        /// <summary>
        /// Returns a representation of the object for the given culture, as a <see cref="string"/>.
        /// </summary>
        /// <param name="culture">A culture name.</param>
        /// <returns>A representation of the object for the given culture, as a <see cref="string"/>.</returns>
        string ToString(string culture);
    }
}
