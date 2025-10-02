namespace Arkanis.Overlay.LocalLink.Services;

using System.IO.Pipes;
using System.Text.Json;
using Common;
using Exceptions;
using Microsoft.Extensions.Logging;
using Models;

public class NamedPipeCommandServer(ILogger<NamedPipeCommandServerBackgroundPublisherService> logger)
{
#if WINDOWS
    public const string PipeName = $"{ApplicationConstants.Company.Slug}/{ApplicationConstants.ApplicationSlug}/LocalLink/Commands";
#else
    public const string PipeName = $"/tmp/{ApplicationConstants.Company.Slug}/{ApplicationConstants.ApplicationSlug}/LocalLink/Commands.pipe";
#endif

    public async Task<LocalLinkCommandBase> ReceiveAsync(TimeSpan communicationTimeout, CancellationToken cancellationToken)
    {
#if !WINDOWS
        Directory.CreateDirectory(Path.GetDirectoryName(PipeName)!);
#endif

        await using var pipe = new NamedPipeServerStream(PipeName, PipeDirection.In);

        logger.LogDebug("Waiting for incoming named pipe connection: {PipeName}", PipeName);
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
            logger.LogWarning(e, "Failed to receive and process a LocalLink command in time");
            throw new LocalLinkConnectionException("The sender took too long to send a LocalLink command.", e);
        }
    }
}
