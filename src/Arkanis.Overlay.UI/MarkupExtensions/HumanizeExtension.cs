namespace Arkanis.Overlay.UI.MarkupExtensions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Humanizer;

public class HumanizeExtension(object? value) : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
        => value switch
        {
            BindingBase source => new MultiBinding
            {
                Bindings = [source],
                Converter = new ValueConverter(),
            },
            _ => value?.ToString() ?? string.Empty,
        };

    private class ValueConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
            => values.First() switch
            {
                TimeSpan value => value.Humanize(culture: culture),
                string value => value.Humanize(),
                var obj => obj?.ToString()?.Humanize(),
            };
    }
}
