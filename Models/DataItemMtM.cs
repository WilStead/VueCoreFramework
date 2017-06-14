namespace MVCCoreVue.Models
{
    /// <summary>
    /// Represents a Many-to-Many join table entity class.
    /// </summary>
    /// <remarks>
    /// All many-to-many join table entity classes must implement this (empty) interface in order for
    /// the framework to correctly detect the relationship.
    ///
    /// The navigation properties of an IDataItemMtM object *must* be named exactly the same as the
    /// navigation properties of the entities they join. This allows the framework to find the
    /// correct properties in the relationship. Changing from a plural navigation property name to a
    /// singular property in the IDataItemMtM is allowed, but be aware that although care was taken
    /// to account for most English words, the singular form of highly unusually pluralized words,
    /// and any non-English words, may not be recognized).
    /// </remarks>
    public interface IDataItemMtM { }
}
