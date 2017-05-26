using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVCCoreVue.Data
{
    interface IRepository<T> where T: DataItem
    {
        Task<T> AddAsync(T item);

        Task<T> FindAsync(Guid id);

        IEnumerable<T> GetAll();

        IEnumerable<T> GetPage(
            string search,
            string sortBy,
            bool descending,
            int page,
            int rowsPerPage);

        Task<int> GetTotalAsync();

        Task RemoveAsync(Guid id);

        Task RemoveRangeAsync(IEnumerable<Guid> ids);

        Task<T> UpdateAsync(T item);
    }
}
