namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using System.Threading.RateLimiting;
using Polly;
using Polly.Retry;

public static class UexSharedResiliency
{
    public const int ApiRequestBatchSize = 10;

    public const int MaxQueueSize = 1_000;
    private static readonly TimeSpan TrackedWindow = TimeSpan.FromSeconds(10);

    private static readonly RateLimiter RateLimiter = new SlidingWindowRateLimiter(
        new SlidingWindowRateLimiterOptions
        {
            Window = TrackedWindow,
            PermitLimit = 50,
            QueueLimit = MaxQueueSize,
            SegmentsPerWindow = 4,
            AutoReplenishment = true,
        }
    );

    private static RetryStrategyOptions RetryOptions { get; } = new()
    {
        Delay = TrackedWindow / 2,
        BackoffType = DelayBackoffType.Linear,
        MaxRetryAttempts = 4,
    };

    public static readonly ResiliencePipeline Pipeline = new ResiliencePipelineBuilder()
        .AddRetry(RetryOptions)
        .AddConcurrencyLimiter(5, MaxQueueSize)
        .AddRateLimiter(RateLimiter)
        .Build();
}
