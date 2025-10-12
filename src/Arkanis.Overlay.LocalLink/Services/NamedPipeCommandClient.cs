namespace Arkanis.Overlay.LocalLink.Services;

using System.IO.Pipes;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Models;

public class NamedPipeCommandClient(ILogger<NamedPipeCommandClient> logger)
{
    public async Task SendAsync(LocalLinkCommandBase command, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        var pipeName = NamedPipeCommandServer.PipeName;
        logger.LogDebug("Connecting via LocalLink: {PipeName}", pipeName);
        await using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
        await pipe.ConnectAsync(cts.Token);

        logger.LogDebug("Sending via LocalLink: {Command}", command);
        await JsonSerializer.SerializeAsync(pipe, command, LocalLinkConstants.SerializerOptions, cts.Token);
        await pipe.FlushAsync(cts.Token);
    }
}
