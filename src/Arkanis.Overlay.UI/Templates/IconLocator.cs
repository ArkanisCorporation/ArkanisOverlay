namespace Arkanis.Overlay.UI.Templates;

using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;
using Models;

public class IconLocator : IDataTemplate
{
    public Control Build(object? param)
        => param switch
        {
            MaterialLibrarySymbol materialSymbol => new MaterialIconText
            {
                Kind = materialSymbol.Kind,
                Text = materialSymbol.Text,
            },
            _ => new MaterialIcon
            {
                Kind = MaterialIconKind.WarningBoxOutline,
                Foreground = Brushes.Red,
            },
        };

    public bool Match(object? data)
        => data is Icon;
}
