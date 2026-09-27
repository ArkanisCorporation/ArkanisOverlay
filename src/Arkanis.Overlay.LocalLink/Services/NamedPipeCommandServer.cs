namespace Arkanis.Overlay.LocalLink.Services;

using System.IO.Pipes;
using System.Text.Json;
using Common;
using Exceptions;
using Microsoft.Extensions.Logging;
using Models;

public class NamedPipeCommandServer
{
    private readonly ILogger<NamedPipeCommandServerBackgroundPublisherService> _logger;
    private readonly string _pipeName;

    public static string PipeName { get; } = ApplicationConstants.IsWindowsPlatform
        ? $"{ApplicationConstants.Company.Slug}/{ApplicationConstants.ApplicationSlug}/LocalLink/Commands"
        : $"/tmp/{ApplicationConstants.Company.Slug}/{ApplicationConstants.ApplicationSlug}/LocalLink/Commands.pipe";

    public NamedPipeCommandServer(ILogger<NamedPipeCommandServerBackgroundPublisherService> logger)
        : this(logger, PipeName)
    {
    }

    internal NamedPipeCommandServer(ILogger<NamedPipeCommandServerBackgroundPublisherService> logger, string pipeName)
    {
        _logger = logger;
        _pipeName = pipeName;
    }

    public virtual async Task<LocalLinkCommandBase> ReceiveAsync(TimeSpan communicationTimeout, CancellationToken cancellationToken)
    {
        if (!ApplicationConstants.IsWindowsPlatform)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_pipeName)!);
        }

        await using var pipe = new NamedPipeServerStream(_pipeName, PipeDirection.In);

        _logger.LogDebug("Waiting for incoming named pipe connection: {PipeName}", _pipeName);
        await pipe.WaitForConnectionAsync(cancellationToken);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(communicationTimeout);

        try
        {
            using var reader = new StreamReader(pipe);
            var commandJson = await reader.ReadToEndAsync(cts.Token);
            var commandBase = JsonSerializer.Deserialize<LocalLinkCommandBase>(commandJson, LocalLinkConstants.SerializerOptions);
            if (commandBase is null)
            {
                throw new LocalLinkReceiveException("Failed to deserialize a LocalLink command.");
            }

            return commandBase;
        }
        catch (OperationCanceledException e)
        {
            _logger.LogWarning(e, "Failed to receive and process a LocalLink command in time");
            throw new LocalLinkConnectionException("The sender took too long to send a LocalLink command.", e);
        }
    }
}
