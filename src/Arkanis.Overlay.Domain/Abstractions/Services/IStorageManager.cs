namespace Arkanis.Overlay.Domain.Abstractions.Services;

public interface IStorageManager
{
    ValueTask WipeAsync(CancellationToken cancellationToken = default);
}
