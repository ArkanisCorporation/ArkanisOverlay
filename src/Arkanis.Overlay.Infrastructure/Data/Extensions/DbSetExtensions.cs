namespace Arkanis.Overlay.Infrastructure.Data.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public static class DbSetExtensions
{
    // See: https://stackoverflow.com/a/76121820/4161937
    public static EntityEntry<TEntity> AddOrUpdate<TEntity>(this DbSet<TEntity> dbSet, TEntity entity)
        where TEntity : class
    {
        var exists = dbSet.Any(c => entity == c);
        return exists
            ? dbSet.Update(entity)
            : dbSet.Add(entity);
    }
}
