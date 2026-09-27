namespace Arkanis.Overlay.LocalLink.Services;

using System.IO.Pipes;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Models;

public class NamedPipeCommandClient
{
    private readonly ILogger<NamedPipeCommandClient> _logger;
    private readonly string _pipeName;

    public NamedPipeCommandClient(ILogger<NamedPipeCommandClient> logger)
        : this(logger, NamedPipeCommandServer.PipeName)
    {
    }

    internal NamedPipeCommandClient(ILogger<NamedPipeCommandClient> logger, string pipeName)
    {
        _logger = logger;
        _pipeName = pipeName;
    }

    public async Task SendAsync(LocalLinkCommandBase command, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        _logger.LogDebug("Connecting via LocalLink: {PipeName}", _pipeName);
        await using var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out);
        await pipe.ConnectAsync(cts.Token);

        _logger.LogDebug("Sending via LocalLink: {Command}", command);
        await JsonSerializer.SerializeAsync(pipe, command, LocalLinkConstants.SerializerOptions, cts.Token);
        await pipe.FlushAsync(cts.Token);
    }
}
