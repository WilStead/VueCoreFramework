using MVCCoreVue.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace MVCCoreVue.Data
{
    interface IRepository
    {
        Task<IDictionary<string, object>> AddAsync(DataItem item);

        Task<IDictionary<string, object>> AddToParentCollectionAsync(DataItem parent, PropertyInfo childProp, IEnumerable<DataItem> children);

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

        Task<IDictionary<string, object>> RemoveChildrenFromCollectionAsync(DataItem parent, PropertyInfo childProp, IEnumerable<DataItem> children);

        Task RemoveRangeAsync(IEnumerable<Guid> ids);

        Task<IDictionary<string, object>> UpdateAsync(DataItem item);
    }
}
