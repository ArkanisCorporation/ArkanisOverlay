namespace Arkanis.Overlay.Common.Extensions;

using Humanizer;

public static class DateTimeOffsetExtensions
{
    public static string ToRelativeString(this DateTimeOffset dateTimeOffset)
    {
        var timeSpan = DateTimeOffset.Now - dateTimeOffset;
        return timeSpan.Humanize();
    }
}
