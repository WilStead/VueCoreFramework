using MVCCoreVue.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MVCCoreVue.Data
{
    /// <summary>
    /// Represents operations with an <see cref="ApplicationDbContext"/> for a particular class.
    /// </summary>
    interface IRepository
    {
        /// <summary>
        /// Asynchronously creates a new instance of <see cref="T"/> and adds it to the <see
        /// cref="ApplicationDbContext"/> instance.
        /// </summary>
        /// <param name="childProp">
        /// An optional navigation property which will be set on the new object.
        /// </param>
        /// <param name="parentId">
        /// The primary key of the entity which will be set on the <paramref name="childProp"/> property.
        /// </param>
        /// <returns>A ViewModel instance representing the newly added entity.</returns>
        Task<IDictionary<string, object>> AddAsync(PropertyInfo childProp, Guid? parentId);

        /// <summary>
        /// Asynchronously adds an assortment of child entities to a parent entity under the given
        /// navigation property.
        /// </summary>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property to which the children will be added.</param>
        /// <param name="childIds">The primary keys of the child entities which will be added.</param>
        Task AddChildrenToCollectionAsync(Guid id, PropertyInfo childProp, IEnumerable<Guid> children);

        /// <summary>
        /// Finds an entity with the given primary key value and returns a ViewModel for that entity.
        /// If no entity is found, an empty ViewModel is returned (not null).
        /// </summary>
        /// <param name="id">The primary key of the entity to be found.</param>
        /// <returns>A ViewModel representing the item found, or an empty ViewModel if none is found.</returns>
        Task<IDictionary<string, object>> FindAsync(Guid id);

        /// <summary>
        /// Finds an entity with the given primary key value. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="id">The primary key of the entity to be found.</param>
        /// <returns>
        /// The item found, or null if none is found.
        /// </returns>
        Task<DataItem> FindItemAsync(Guid id);

        /// <summary>
        /// Enumerates all the entities in the <see cref="ApplicationDbContext"/>'s set, returning a
        /// ViewModel representing each.
        /// </summary>
        /// <returns>ViewModels representing the items in the set.</returns>
        IEnumerable<IDictionary<string, object>> GetAll();

        /// <summary>
        /// Finds the primary key of a child entity in the given relationship.
        /// </summary>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property of the relationship.</param>
        /// <returns></returns>
        Task<Guid> GetChildIdAsync(Guid id, PropertyInfo childProp);

        /// <summary>
        /// Retrieves the total number of child entities in the given relationship.
        /// </summary>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property of the relationship on the parent entity.</param>
        /// <returns>
        /// A <see cref="long"/> that represents the total number of children in the relationship.
        /// </returns>
        Task<long> GetChildTotalAsync(Guid id, PropertyInfo childProp);

        /// <summary>
        /// Generates and enumerates <see cref="FieldDefinition"/> s representing the properties of
        /// <see cref="T"/>.
        /// </summary>
        IEnumerable<FieldDefinition> GetFieldDefinitions();

        /// <summary>
        /// Calculates and enumerates the set of entities with the given paging parameters, as ViewModels.
        /// </summary>
        /// <param name="search">
        /// An optional search term which will filter the results. Any string or numeric property
        /// with matching text will be included.
        /// </param>
        /// <param name="sortBy">
        /// An optional property name which will be used to sort the items before calculating the
        /// page contents.
        /// </param>
        /// <param name="descending">
        /// Indicates whether the sort is descending; if false, the sort is ascending.
        /// </param>
        /// <param name="page">The page number requested.</param>
        /// <param name="rowsPerPage">The number of items per page.</param>
        /// <param name="except">
        /// An enumeration of primary keys of items which should be excluded from the results before
        /// caluclating the page contents.
        /// </param>
        IEnumerable<IDictionary<string, object>> GetPage(
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage,
            IEnumerable<Guid> except,
            IList<Claim> claims);

        /// <summary>
        /// Asynchronously returns a <see cref="long"/> that represents the total number of entities
        /// in the set.
        /// </summary>
        Task<long> GetTotalAsync();

        /// <summary>
        /// Asynchronously removes an entity from the <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="id">The primary key of the entity to remove.</param>
        Task RemoveAsync(Guid id);

        /// <summary>
        /// Asynchronously removes an assortment of child entities from a parent entity under the
        /// given navigation property.
        /// </summary>
        /// <param name="id">The primary key of the parent entity.</param>
        /// <param name="childProp">The navigation property from which the children will be removed.</param>
        /// <param name="childIds">The primary keys of the child entities which will be removed.</param>
        Task RemoveChildrenFromCollectionAsync(Guid id, PropertyInfo childProp, IEnumerable<Guid> childIds);

        /// <summary>
        /// Asynchronously terminates a relationship bewteen two entities. If the child entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="id">The primary key of the child entity whose relationship is being severed.</param>
        /// <param name="childProp">The navigation property of the relationship being severed.</param>
        Task<bool> RemoveFromParentAsync(Guid id, PropertyInfo childProp);

        /// <summary>
        /// Asynchronously removes a collection of entities from the <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="ids">An enumeration of the primary keys of the entities to remove.</param>
        Task RemoveRangeAsync(IEnumerable<Guid> ids);

        /// <summary>
        /// Asynchronously terminates a relationship for multiple entities. If any child entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="ids">
        /// An enumeration of primary keys of child entities whose relationships are being severed.
        /// </param>
        /// <param name="childProp">The navigation property of the relationship being severed.</param>
        Task<IList<Guid>> RemoveRangeFromParentAsync(IEnumerable<Guid> ids, PropertyInfo childProp);

        /// <summary>
        /// Asynchronously creates a relationship between two entities, replacing another entity
        /// which was previously in that relationship with another one. If the replaced entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="parentId">The primary key of the parent entity in the relationship.</param>
        /// <param name="newChildId">
        /// The primary key of the new child entity entering into the relationship.
        /// </param>
        /// <param name="childProp">The navigation property of the relationship on the child entity.</param>
        Task<Guid?> ReplaceChildAsync(Guid parentId, Guid newChildId, PropertyInfo childProp);

        /// <summary>
        /// Asynchronously creates a relationship between two entities, replacing another entity
        /// which was previously in that relationship with a new entity. If the replaced entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="parentId">The primary key of the parent entity in the relationship.</param>
        /// <param name="childProp">The navigation property of the relationship on the child entity.</param>
        Task<(IDictionary<string, object>, Guid?)> ReplaceChildWithNewAsync(Guid parentId, PropertyInfo childProp);

        /// <summary>
        /// Asynchronously updates an entity in the <see cref="ApplicationDbContext"/>. Returns a
        /// ViewModel representing the updated item.
        /// </summary>
        /// <param name="item">The item to update.</param>
        /// <returns>A ViewModel representing the updated item.</returns>
        Task<IDictionary<string, object>> UpdateAsync(DataItem item);
    }
}
