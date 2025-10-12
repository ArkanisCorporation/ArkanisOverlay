namespace Arkanis.Overlay.Domain.Abstractions.Services;

public interface IStorageManager
{
    public ValueTask WipeAsync(CancellationToken cancellationToken = default);
}
