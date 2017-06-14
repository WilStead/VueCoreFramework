using MVCCoreVue.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace MVCCoreVue.Data
{
    interface IRepository
    {
        Task<IDictionary<string, object>> AddAsync(PropertyInfo childProp, Guid? parentId);

        Task AddChildrenToCollectionAsync(Guid id, PropertyInfo childProp, IEnumerable<Guid> children);

        Task<IDictionary<string, object>> FindAsync(Guid id);

        Task<DataItem> FindItemAsync(Guid id);

        IEnumerable<IDictionary<string, object>> GetAll();

        IEnumerable<FieldDefinition> GetFieldDefinitions();

        IEnumerable<IDictionary<string, object>> GetPage(
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage,
            IEnumerable<Guid> except);

        Task<long> GetTotalAsync();

        Task RemoveAsync(Guid id);

        Task RemoveChildrenFromCollectionAsync(Guid id, PropertyInfo childProp, IEnumerable<Guid> childIds);

        Task RemoveFromParentAsync(Guid id, PropertyInfo childProp);

        Task RemoveRangeAsync(IEnumerable<Guid> ids);

        Task RemoveRangeFromParentAsync(IEnumerable<Guid> ids, PropertyInfo childProp);

        Task ReplaceChildAsync(Guid parentId, Guid newChildId, PropertyInfo childProp);

        Task<IDictionary<string, object>> ReplaceChildWithNewAsync(Guid parentId, PropertyInfo childProp);

        Task<IDictionary<string, object>> UpdateAsync(DataItem item);
    }
}
