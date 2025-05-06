namespace Arkanis.Common.Extensions;

using System.Text;
using MoreLinq;

public static class StringExtensions
{
    private const StringSplitOptions SplitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

    public static string Abbreviate(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        str.Split(' ', SplitOptions)
            .Select(part => part[0])
            .ForEach(firstLetter => builder.Append(firstLetter));

        return builder.ToString();
    }
}
