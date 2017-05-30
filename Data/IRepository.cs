using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVCCoreVue.Data
{
    interface IRepository
    {
        Task<object> AddAsync(object item);

        Task<object> FindAsync(Guid id);

        IEnumerable<object> GetAll();

        IEnumerable<FieldDefinition> GetFieldDefinitions();

        IEnumerable<object> GetPage(
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage);

        Task<long> GetTotalAsync();

        Task RemoveAsync(Guid id);

        Task RemoveRangeAsync(IEnumerable<Guid> ids);

        Task<object> UpdateAsync(object item);
    }
}
