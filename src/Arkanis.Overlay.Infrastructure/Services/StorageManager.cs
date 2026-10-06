namespace Arkanis.Overlay.Infrastructure.Services;

using Common;
using Domain.Abstractions.Services;
using Microsoft.Extensions.Logging;

public class StorageManager(ILogger<StorageManager> logger) : IStorageManager
{
    public ValueTask WipeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogWarning("Processing complete storage wipe");
            ApplicationConstants.ApplicationDataDirectory.Delete(true);
        }
        catch (IOException e)
        {
            logger.LogWarning(e, "Some files failed to be removed");
        }

        return ValueTask.CompletedTask;
    }
}
