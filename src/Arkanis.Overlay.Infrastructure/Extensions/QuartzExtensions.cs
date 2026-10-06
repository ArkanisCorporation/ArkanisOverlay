namespace Arkanis.Overlay.Infrastructure.Extensions;

using Quartz;

public static class QuartzExtensions
{
    public static T GetFromJobData<T>(this IJobExecutionContext context, string key)
        => context.MergedJobDataMap.Get<T>(key);

    public static T Get<T>(this JobDataMap dataMap, string key)
    {
        if (!dataMap.TryGetValue(key, out var value))
        {
            throw new ArgumentException($"Unable to retrieve required value from job data: {key} (contains keys: {string.Join(", ", dataMap.Keys)})");
        }

        if (value is not T result)
        {
            throw new ArgumentException($"Retrieved job data value is not of requested type: {typeof(T)}, but instead: {value.GetType()}");
        }

        return result;
    }
}
