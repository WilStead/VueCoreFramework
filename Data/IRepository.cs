using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace MVCCoreVue.Data
{
    interface IRepository
    {
        Task<object> AddAsync(object item);

        Task<object> AddChildAsync(object item, object child, PropertyInfo pInfo, PropertyInfo idPInfo);

        Task<object> FindAsync(Guid id);

        Task<object> FindItemAsync(Guid id);

        IEnumerable<object> GetAll();

        IEnumerable<FieldDefinition> GetFieldDefinitions();

        IEnumerable<object> GetPage(
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage);

        Task<long> GetTotalAsync();

        Task<object> NewAsync();

        Task RemoveAsync(Guid id);

        Task<object> RemoveChildAsync(object item, PropertyInfo pInfo, PropertyInfo idPInfo);

        Task RemoveRangeAsync(IEnumerable<Guid> ids);

        Task<object> UpdateAsync(object item);
    }
}
