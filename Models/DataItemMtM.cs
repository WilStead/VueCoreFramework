namespace MVCCoreVue.Models
{
    /// <summary>
    /// Represents a Many-to-Many join table entity class.
    /// </summary>
    /// <remarks>
    /// All Many-to-many join table entity classes must implement this (empty) interface in order for
    /// the framework to correctly detect the relationship.
    ///
    /// The navigation properties of an IDataItemMtM object *must* be named exactly the same as the
    /// navigation properties of the entities they join (change of pluralization is not allowed).
    /// This allows the framework to find the correct properties in the relationship.
    /// </remarks>
    public interface IDataItemMtM { }
}
