namespace Arkanis.Overlay.Infrastructure.Services.Abstractions;

using Domain.Abstractions;

public interface ISelfInitializable : IDependable
{
    Task InitializeAsync(CancellationToken cancellationToken);
}
