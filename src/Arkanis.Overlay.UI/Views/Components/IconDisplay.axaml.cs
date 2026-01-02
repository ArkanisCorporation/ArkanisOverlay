namespace Arkanis.Overlay.UI.Views.Components;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Models;

[PseudoClasses(":invalid", ":has-text")]
public class IconDisplay : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<IconDisplay, string?>(nameof(Text));

    public static readonly StyledProperty<Icon?> IconProperty =
        AvaloniaProperty.Register<IconDisplay, Icon?>(nameof(Icon), MissingIcon.Instance);

    public static readonly StyledProperty<double?> IconSizeProperty =
        AvaloniaProperty.Register<IconDisplay, double?>(nameof(IconSize));

    public static readonly StyledProperty<double> IconSizeRatioProperty =
        AvaloniaProperty.Register<IconDisplay, double>(nameof(IconSizeRatio), 1);

    public static readonly StyledProperty<double> EffectiveIconSizeProperty =
        AvaloniaProperty.Register<IconDisplay, double>(nameof(EffectiveIconSize));

    public IconDisplay()
    {
        IconProperty.Changed.AddClassHandler<IconDisplay>((_, e) => SetIconPseudoClasses(e.NewValue));
        TextProperty.Changed.AddClassHandler<IconDisplay>((_, e) => SetTextPseudoClasses(e.NewValue));
        FontSizeProperty.Changed.AddClassHandler<IconDisplay>((c, _) => c.UpdateEffectiveIconSize());
        IconSizeProperty.Changed.AddClassHandler<IconDisplay>((c, _) => c.UpdateEffectiveIconSize());
        IconSizeRatioProperty.Changed.AddClassHandler<IconDisplay>((c, _) => c.UpdateEffectiveIconSize());
    }

    public Icon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    ///     Explicit icon size override. When null, FontSize is used.
    /// </summary>
    public double? IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public double IconSizeRatio
    {
        get => GetValue(IconSizeRatioProperty);
        set => SetValue(IconSizeRatioProperty, value);
    }

    internal double EffectiveIconSize
    {
        get => GetValue(EffectiveIconSizeProperty);
        private set => SetValue(EffectiveIconSizeProperty, value);
    }

    private void SetIconPseudoClasses(object? icon)
        => PseudoClasses.Set(":invalid", icon is null);

    private void SetTextPseudoClasses(object? text)
        => PseudoClasses.Set(":has-text", text is not null);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateEffectiveIconSize();
        SetIconPseudoClasses(Icon);
        SetTextPseudoClasses(Text);
    }

    private void UpdateEffectiveIconSize()
        => EffectiveIconSize = IconSize ?? IconSizeRatio * FontSize;
}
