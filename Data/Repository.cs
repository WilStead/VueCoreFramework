using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MVCCoreVue.Data
{
    public class Repository<T> : IRepository<T> where T : DataItem
    {
        private readonly ApplicationDbContext _context;

        private DbSet<T> items;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            items = _context.Set<T>();
        }

        public async Task<T> AddAsync(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            await items.AddAsync(item);
            await _context.SaveChangesAsync();
            return item;
        }

        private bool AnyPropMatch(T item, string search)
        {
            var type = typeof(T);
            foreach (var pInfo in type.GetProperties())
            {
                // Only strings and numbers are checked, to avoid any
                // potentially expensive ToString operations on
                // potentially many rows.
                if (pInfo.PropertyType == typeof(string))
                {
                    if ((pInfo.GetValue(item) as string).IndexOf(search) != -1)
                    {
                        return true;
                    }
                }
                else if (pInfo.PropertyType == typeof(short)
                    || pInfo.PropertyType == typeof(int)
                    || pInfo.PropertyType == typeof(long)
                    || pInfo.PropertyType == typeof(float)
                    || pInfo.PropertyType == typeof(double)
                    || pInfo.PropertyType == typeof(decimal))
                {
                    if (pInfo.GetValue(item).ToString().IndexOf(search) != -1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<T> FindAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            return await items.FindAsync(id);
        }

        public IEnumerable<T> GetAll()
        {
            return items.AsEnumerable();
        }

        public IEnumerable<T> GetPage(string search, string sortBy, bool descending, int page, int rowsPerPage)
        {
            IQueryable<T> filteredItems = items.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                filteredItems = items.Where(i => AnyPropMatch(i, search));
            }
            
            if (!string.IsNullOrEmpty(sortBy))
            {
                var sortProp = typeof(T).GetProperty(sortBy);
                if (sortProp == null)
                {
                    throw new ArgumentException($"{sortBy} is not a valid property for this item.", nameof(sortBy));
                }
                if (descending)
                {
                    filteredItems = filteredItems.OrderByDescending(i => sortProp.GetValue(i));
                }
                else
                {
                    filteredItems = filteredItems.OrderBy(i => sortProp.GetValue(i));
                }
            }

            if (rowsPerPage > 0)
            {
                if (page < 1)
                {
                    throw new ArgumentException($"{nameof(page)} cannot be < 1 if {nameof(rowsPerPage)} is > 0.", nameof(page));
                }
                filteredItems = filteredItems.Skip((page - 1) * rowsPerPage).Take(page * rowsPerPage);
            }

            return filteredItems.AsEnumerable();
        }

        public async Task<int> GetTotalAsync()
        {
            return await items.CountAsync();
        }

        public async Task RemoveAsync(Guid id)
        {
            var item = await FindAsync(id);
            items.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<Guid> ids)
        {
            items.RemoveRange(ids.Select(i => items.Find(i)));
            await _context.SaveChangesAsync();
        }

        public async Task<T> UpdateAsync(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            items.Update(item);
            await _context.SaveChangesAsync();
            return item;
        }
    }
}
