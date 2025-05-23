namespace Arkanis.Overlay.Infrastructure.UnitTests.CacheDecorators;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

public abstract class ServiceCacheDecorator(ILogger logger)
{
    private const string CacheDirName = ".data";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly Action<ILogger, string, string, Exception?> _logCacheRead = LoggerMessage.Define<string, string>(
        LogLevel.Debug,
        new EventId(),
        "Loading cached result for a call to {MethodName} from: {FilePath}"
    );

    private readonly Action<ILogger, string, string, Exception?> _logCacheWrite = LoggerMessage.Define<string, string>(
        LogLevel.Debug,
        new EventId(),
        "Writing cached result from a call of {MethodName} to: {FilePath}"
    );

    private readonly Action<ILogger, string, object?, Exception?> _logCall = LoggerMessage.Define<string, object?>(
        LogLevel.Debug,
        new EventId(),
        "Proxying call to {MethodName} with params: {@MethodParams}"
    );

    private readonly Action<ILogger, string, object?, Exception?> _logLiveCall = LoggerMessage.Define<string, object?>(
        LogLevel.Information,
        new EventId(),
        "Cache not resolved, performing live call of {MethodName} with params: {@MethodParams}"
    );

    protected abstract string CacheSubPath { get; }

    [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms")]
    protected async Task<TResult> CacheAsync<TParams, TResult>(
        TParams methodParams,
        string methodName,
        Func<TParams, Task<TResult>> runAsync,
        [CallerFilePath] string callerFilePath = ""
    )
    {
        var serializedParams = JsonSerializer.Serialize(methodParams);
        _logCall(logger, methodName, methodParams, null);

        var paramsId = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(serializedParams)));
        TResult? result = default;

        var callerDirPath = Path.GetDirectoryName(callerFilePath);
        var cacheFileName = $"{methodName}-{paramsId}.json";
        var cacheFileDir = !string.IsNullOrWhiteSpace(CacheSubPath)
            ? Path.Join(callerDirPath, CacheDirName, CacheSubPath)
            : Path.Join(callerDirPath, CacheDirName);

        var cacheFilePath = Path.Join(cacheFileDir, cacheFileName);
        if (File.Exists(cacheFilePath))
        {
            _logCacheRead(logger, methodName, cacheFilePath, null);
            result = await JsonSerializer.DeserializeAsync<TResult>(File.OpenRead(cacheFilePath), SerializerOptions);
        }

        if (result is null)
        {
            _logLiveCall(logger, methodName, methodParams, null);
            result = await runAsync(methodParams);
            _logCacheWrite(logger, methodName, cacheFilePath, null);

            Directory.CreateDirectory(cacheFileDir);
            await using var file = File.OpenWrite(cacheFilePath);
            await JsonSerializer.SerializeAsync(file, result, SerializerOptions);
        }

        return result;
    }
}
