using VueCoreFramework.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VueCoreFramework.Data
{
    /// <summary>
    /// Represents operations with an <see cref="ApplicationDbContext"/> for a particular class.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// The <see cref="IEntityType"/> of this Repository. Read-only.
        /// </summary>
        IEntityType EntityType { get; }

        /// <summary>
        /// The FieldDefinitions for this repository's entity type. Read-only.
        /// </summary>
        List<FieldDefinition> FieldDefinitions { get; }

        /// <summary>
        /// The primary key <see cref="IProperty"/> of this Repository's entity type. Read-only.
        /// </summary>
        IProperty PrimaryKey { get; }

        /// <summary>
        /// The name of the ViewModel property which indicates the primary key. Read-only.
        /// </summary>
        string PrimaryKeyVMProperty { get; }

        /// <summary>
        /// Asynchronously creates a new instance of the repository's type and adds it to the <see
        /// cref="ApplicationDbContext"/> instance.
        /// </summary>
        /// <param name="childProp">
        /// An optional navigation property which will be set on the new object.
        /// </param>
        /// <param name="parentId">
        /// The primary key of the entity which will be set on the <paramref name="childProp"/>
        /// property, as a string.
        /// </param>
        /// <returns>A ViewModel instance representing the newly added entity.</returns>
        Task<IDictionary<string, object>> AddAsync(PropertyInfo childProp, string parentId);

        /// <summary>
        /// Asynchronously creates a new instance of the repository's type and adds it to the <see
        /// cref="ApplicationDbContext"/> instance.
        /// </summary>
        /// <param name="childProp">
        /// An optional navigation property which will be set on the new object.
        /// </param>
        /// <param name="parentId">
        /// The primary key of the entity which will be set on the <paramref name="childProp"/>
        /// property, as a string.
        /// </param>
        /// <param name="culture">A string indicating the current culture.</param>
        /// <returns>A ViewModel instance representing the newly added entity.</returns>
        Task<IDictionary<string, object>> AddAsync(PropertyInfo childProp, string parentId, string culture);

        /// <summary>
        /// Asynchronously adds an assortment of child entities to a parent entity under the given
        /// navigation property.
        /// </summary>
        /// <param name="id">The primary key of the parent entity, as a string.</param>
        /// <param name="childProp">The navigation property to which the children will be added.</param>
        /// <param name="children">The primary keys of the child entities which will be added, as strings.</param>
        Task AddChildrenToCollectionAsync(string id, PropertyInfo childProp, IEnumerable<string> children);

        /// <summary>
        /// Asynchronously duplicates an entity in the <see cref="ApplicationDbContext"/>. Returns a
        /// ViewModel representing the new copy.
        /// </summary>
        /// <param name="id">The primary key of the entity to be copied, as a string.</param>
        /// <returns>A ViewModel representing the new item.</returns>
        /// <remarks>
        /// All non-navigation properties are copied. Navigation properties for one-to-many or
        /// many-to-many relationships are duplicated. Navigation properties for other
        /// relationships are left null (since the relationship forbids having more than one).
        /// </remarks>
        Task<IDictionary<string, object>> DuplicateAsync(string id);

        /// <summary>
        /// Asynchronously duplicates an entity in the <see cref="ApplicationDbContext"/>. Returns a
        /// ViewModel representing the new copy.
        /// </summary>
        /// <param name="id">The primary key of the entity to be copied, as a string.</param>
        /// <param name="culture">A string indicating the current culture.</param>
        /// <returns>A ViewModel representing the new item.</returns>
        /// <remarks>
        /// All non-navigation properties are copied. Navigation properties for one-to-many or
        /// many-to-many relationships are duplicated. Navigation properties for other
        /// relationships are left null (since the relationship forbids having more than one).
        /// </remarks>
        Task<IDictionary<string, object>> DuplicateAsync(string id, string culture);

        /// <summary>
        /// Finds an entity with the given primary key value and returns a ViewModel for that entity.
        /// If no entity is found, null is returned.
        /// </summary>
        /// <param name="id">The primary key of the entity to be found, as a string.</param>
        /// <returns>A ViewModel representing the item found, or an empty ViewModel if none is found.</returns>
        Task<IDictionary<string, object>> FindAsync(string id);

        /// <summary>
        /// Finds an entity with the given primary key value and returns a ViewModel for that entity.
        /// If no entity is found, null is returned.
        /// </summary>
        /// <param name="id">The primary key of the entity to be found, as a string.</param>
        /// <param name="culture">A string indicating the current culture.</param>
        /// <returns>A ViewModel representing the item found, or an empty ViewModel if none is found.</returns>
        Task<IDictionary<string, object>> FindAsync(string id, string culture);

        /// <summary>
        /// Finds an entity with the given primary key value. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="id">The primary key of the entity to be found, as a string.</param>
        /// <returns>
        /// The item found, or null if none is found.
        /// </returns>
        Task<object> FindItemAsync(string id);

        /// <summary>
        /// Finds an entity with the given primary key value. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="key">The primary key of the entity to be found.</param>
        /// <returns>
        /// The item found, or null if none is found.
        /// </returns>
        Task<object> FindItemWithPKAsync(object key);

        /// <summary>
        /// Enumerates all the entities in the <see cref="ApplicationDbContext"/>'s set, returning a
        /// ViewModel representing each.
        /// </summary>
        /// <returns>ViewModels representing the items in the set.</returns>
        Task<IList<IDictionary<string, object>>> GetAllAsync();

        /// <summary>
        /// Enumerates all the entities in the <see cref="ApplicationDbContext"/>'s set, returning a
        /// ViewModel representing each.
        /// </summary>
        /// <param name="culture">A string indicating the current culture.</param>
        /// <returns>ViewModels representing the items in the set.</returns>
        Task<IList<IDictionary<string, object>>> GetAllAsync(string culture);

        /// <summary>
        /// Finds the primary keys of all child entities in the given relationship, as strings.
        /// </summary>
        /// <param name="id">The primary key of the parent entity, as a string.</param>
        /// <param name="childProp">The navigation property of the relationship.</param>
        Task<IList<string>> GetAllChildIdsAsync(string id, PropertyInfo childProp);

        /// <summary>
        /// Finds the primary key of a child entity in the given relationship, as a string.
        /// </summary>
        /// <param name="id">The primary key of the parent entity, as a string.</param>
        /// <param name="childProp">The navigation property of the relationship.</param>
        Task<string> GetChildIdAsync(string id, PropertyInfo childProp);

        /// <summary>
        /// Calculates and enumerates the set of child entities in a given relationship with the
        /// given paging parameters, as ViewModels.
        /// </summary>
        /// <param name="id">The primary key of the parent entity, as a string.</param>
        /// <param name="childProp">The navigation property of the relationship on the parent entity.</param>
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
        /// <param name="claims">The collection of claims held by the current user.</param>
        Task<IList<IDictionary<string, object>>> GetChildPageAsync(
            string id,
            PropertyInfo childProp,
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage,
            IList<Claim> claims);

        /// <summary>
        /// Calculates and enumerates the set of child entities in a given relationship with the
        /// given paging parameters, as ViewModels.
        /// </summary>
        /// <param name="id">The primary key of the parent entity, as a string.</param>
        /// <param name="childProp">The navigation property of the relationship on the parent entity.</param>
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
        /// <param name="claims">The collection of claims held by the current user.</param>
        /// <param name="culture">A string indicating the current culture.</param>
        Task<IList<IDictionary<string, object>>> GetChildPageAsync(
            string id,
            PropertyInfo childProp,
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage,
            IList<Claim> claims,
            string culture);

        /// <summary>
        /// Retrieves the total number of child entities in the given relationship.
        /// </summary>
        /// <param name="id">The primary key of the parent entity, as a string.</param>
        /// <param name="childProp">The navigation property of the relationship on the parent entity.</param>
        /// <returns>
        /// A <see cref="long"/> that represents the total number of children in the relationship.
        /// </returns>
        Task<long> GetChildTotalAsync(string id, PropertyInfo childProp);

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
        /// calculating the page contents, as strings.
        /// </param>
        /// <param name="claims">The collection of claims held by the current user.</param>
        Task<IList<IDictionary<string, object>>> GetPageAsync(
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage,
            IEnumerable<string> except,
            IList<Claim> claims);

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
        /// calculating the page contents, as strings.
        /// </param>
        /// <param name="claims">The collection of claims held by the current user.</param>
        /// <param name="culture">A string indicating the current culture.</param>
        Task<IList<IDictionary<string, object>>> GetPageAsync(
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage,
            IEnumerable<string> except,
            IList<Claim> claims,
            string culture);

        /// <summary>
        /// Calculates and enumerates the given items with the given paging parameters, as ViewModels.
        /// </summary>
        /// <param name="items">The items to filter, sort, and page.</param>
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
        /// <param name="claims">The collection of claims held by the current user.</param>
        /// <param name="culture">A string indicating the current culture.</param>
        Task<IList<IDictionary<string, object>>> GetPageItemsAsync(
            IQueryable<object> items,
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage,
            IList<Claim> claims,
            string culture);

        /// <summary>
        /// Converts the given string into its equivalent primary key for this type.
        /// </summary>
        /// <param name="pk_string">The primary key to convert, as a string.</param>
        /// <returns>The primary key, as whatever type is defined by the entity.</returns>
        object GetPrimaryKeyFromString(string pk_string);

        /// <summary>
        /// Asynchronously returns a <see cref="long"/> that represents the total number of entities
        /// in the set.
        /// </summary>
        Task<long> GetTotalAsync();

        /// <summary>
        /// Asynchronously removes an entity from the <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="id">The primary key of the entity to remove, as a string.</param>
        Task RemoveAsync(string id);

        /// <summary>
        /// Asynchronously removes an assortment of child entities from a parent entity under the
        /// given navigation property.
        /// </summary>
        /// <param name="id">The primary key of the parent entity, as a string.</param>
        /// <param name="childProp">The navigation property from which the children will be removed.</param>
        /// <param name="childIds">The primary keys of the child entities which will be removed, as strings.</param>
        Task RemoveChildrenFromCollectionAsync(string id, PropertyInfo childProp, IEnumerable<string> childIds);

        /// <summary>
        /// Asynchronously terminates a relationship bewteen two entities. If the child entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="id">The primary key of the child entity whose relationship is being severed, as a string.</param>
        /// <param name="childProp">The navigation property of the relationship being severed.</param>
        Task<bool> RemoveFromParentAsync(string id, PropertyInfo childProp);

        /// <summary>
        /// Asynchronously removes a collection of entities from the <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="ids">An enumeration of the primary keys of the entities to remove, as strings.</param>
        Task RemoveRangeAsync(IEnumerable<string> ids);

        /// <summary>
        /// Asynchronously terminates a relationship for multiple entities. If any child entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="ids">
        /// An enumeration of primary keys of child entities whose relationships are being severed, as strings.
        /// </param>
        /// <param name="childProp">The navigation property of the relationship being severed.</param>
        /// <returns>A list of the Ids of any items removed from the <see cref="ApplicationDbContext"/>, as strings.</returns>
        Task<IList<string>> RemoveRangeFromParentAsync(IEnumerable<string> ids, PropertyInfo childProp);

        /// <summary>
        /// Asynchronously creates a relationship between two entities, replacing another entity
        /// which was previously in that relationship with another one. If the replaced entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="parentId">
        /// The primary key of the parent entity in the relationship, as a string.
        /// </param>
        /// <param name="newChildId">
        /// The primary key of the new child entity entering into the relationship, as a string.
        /// </param>
        /// <param name="childProp">The navigation property of the relationship on the child entity.</param>
        /// <returns>
        /// The Id of the removed child, if it is removed from the <see
        /// cref="ApplicationDbContext"/>, as a string; null if it is not.
        /// </returns>
        Task<string> ReplaceChildAsync(string parentId, string newChildId, PropertyInfo childProp);

        /// <summary>
        /// Asynchronously creates a relationship between two entities, replacing another entity
        /// which was previously in that relationship with a new entity. If the replaced entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="parentId">The primary key of the parent entity in the relationship, as a string.</param>
        /// <param name="childProp">The navigation property of the relationship on the child entity.</param>
        Task<(IDictionary<string, object>, string)> ReplaceChildWithNewAsync(string parentId, PropertyInfo childProp);

        /// <summary>
        /// Asynchronously creates a relationship between two entities, replacing another entity
        /// which was previously in that relationship with a new entity. If the replaced entity is
        /// made an orphan by the removal and is not a MenuClass object, it is then removed from the
        /// <see cref="ApplicationDbContext"/> entirely.
        /// </summary>
        /// <param name="parentId">The primary key of the parent entity in the relationship, as a string.</param>
        /// <param name="childProp">The navigation property of the relationship on the child entity.</param>
        /// <param name="culture">A string indicating the current culture.</param>
        Task<(IDictionary<string, object>, string)> ReplaceChildWithNewAsync(string parentId, PropertyInfo childProp, string culture);

        /// <summary>
        /// Asynchronously updates an entity in the <see cref="ApplicationDbContext"/>. Returns a
        /// ViewModel representing the updated item.
        /// </summary>
        /// <param name="item">The item to update.</param>
        /// <returns>A ViewModel representing the updated item.</returns>
        Task<IDictionary<string, object>> UpdateAsync(object item);

        /// <summary>
        /// Asynchronously updates an entity in the <see cref="ApplicationDbContext"/>. Returns a
        /// ViewModel representing the updated item.
        /// </summary>
        /// <param name="item">The item to update.</param>
        /// <param name="culture">A string indicating the current culture.</param>
        /// <returns>A ViewModel representing the updated item.</returns>
        Task<IDictionary<string, object>> UpdateAsync(object item, string culture);
    }
}
