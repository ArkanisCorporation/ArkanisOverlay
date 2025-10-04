namespace Arkanis.Overlay.LocalLink.Abstractions;

using Models;

public interface ILocalLinkCommandPublisher
{
    public Task PublishAsync(LocalLinkCommandBase localLinkCommand, CancellationToken cancellationToken);
}
