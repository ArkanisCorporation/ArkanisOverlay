namespace Arkanis.Overlay.Host.Desktop.Helpers;

using Microsoft.Toolkit.Uwp.Notifications;

public static class ToastArgumentHelper
{
    public static Dictionary<string, string> Parse(string values)
    {
        var arguments = new Dictionary<string, string>();
        var pairs = values.Split(';');
        foreach (var pair in pairs)
        {
            if (!pair.Contains('='))
            {
                continue;
            }

            var split = pair.Split('=', 2);
            arguments[split[0]] = split[1];
        }

        return arguments;
    }

    public static ToastButton AddArguments(this ToastButton button, Dictionary<string, string> arguments)
    {
        foreach (var (key, value) in arguments)
        {
            button.AddArgument(key, value);
        }

        return button;
    }
}
