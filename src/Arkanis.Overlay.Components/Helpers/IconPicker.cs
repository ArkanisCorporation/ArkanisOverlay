namespace Arkanis.Overlay.Components.Helpers;

using Domain.Abstractions.Services;
using Domain.Enums;

public class IconPicker : IIconPicker
{
    private readonly IIconService _iconService;

    public IconPicker(IIconService iconService)
    {
        _iconService = iconService;
    }

    public string PickIconFor(PriceType value)
        => MudBlazorIconMapping.GetIconString(_iconService.GetIconSelectionFor(value));

    public string PickIconFor(GameEntityCategory value)
        => MudBlazorIconMapping.GetIconString(_iconService.GetIconSelectionFor(value));

    public string PickIconFor<T>(T value)
        => value switch
        {
            GameEntityCategory x => PickIconFor(x),
            PriceType x => PickIconFor(x),
            _ => MudBlazorIconMapping.DefaultIconString,
        };
}
