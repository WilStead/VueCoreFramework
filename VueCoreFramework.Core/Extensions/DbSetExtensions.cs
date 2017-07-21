using Microsoft.EntityFrameworkCore;

namespace VueCoreFramework.Core.Extensions
{
    /// <summary>
    /// Custom extensions for <see cref="DbSet{TEntity}"/>.
    /// </summary>
    public static class DbSetExtensions
    {
        /// <summary>
        /// Removes all rows in the set. WARNING: Not optimized for performance, intended only for
        /// clearing seed and/or test data.
        /// </summary>
        public static void Clear<T>(this DbSet<T> dbSet) where T : class
            => dbSet.RemoveRange(dbSet);
    }
}
