namespace Arkanis.Overlay.Infrastructure.Data.Extensions;

using Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public static class DbSetExtensions
{
    // See: https://stackoverflow.com/a/76121820/4161937
    public static EntityEntry<TEntity> AddOrUpdate<TEntity>(this DbSet<TEntity> dbSet, TEntity currentEntity)
        where TEntity : class, IDatabaseEntity
    {
        var exists = dbSet.Any(existingEntity => existingEntity.Equals(currentEntity));
        return exists
            ? dbSet.Update(currentEntity)
            : dbSet.Add(currentEntity);
    }
}
