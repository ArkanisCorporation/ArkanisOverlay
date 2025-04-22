namespace Arkanis.Overlay.Application.Workers;

using Infrastructure.API;
using Infrastructure.Data;
using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class DataSync(ILogger<DataSync> logger, DataClient dataClient, IServiceProvider serviceProvider)
{
    public async Task Update<T>(string endpoint) where T : BaseEntity, new()
    {
        var typeName = typeof(T).Name;
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UEXContext>();

        logger.LogInformation("Updating {type} from {endpoint}", typeName, endpoint);
        try
        {
            var result = await dataClient.Get<List<T>>(endpoint).ConfigureAwait(false);

            if (result == null)
            {
                return;
            }

            var dbSet = dbContext.Set<T>();
            // we are synchronizing the database from the API
            // so we can be sure that the database is always up to date
            // therefore we delete all existing data and replace it with the new data
            await dbSet.ExecuteDeleteAsync().ConfigureAwait(false);
            await dbSet.AddRangeAsync(result).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            // dbContext.ChangeTracker.Clear();
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to update {type} from {endpoint}: {message}", typeName, endpoint, ex.Message);
        }

        logger.LogInformation("Finished updating {type} from {endpoint}", typeName, endpoint);
    }
}
