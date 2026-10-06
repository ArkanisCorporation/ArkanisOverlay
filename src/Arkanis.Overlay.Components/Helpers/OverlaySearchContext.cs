namespace Arkanis.Overlay.Components.Helpers;

using Domain.Abstractions.Game;
using Microsoft.AspNetCore.Components;
using Views.Components;

public class OverlaySearchContext
{
    public IGameLocation? CurrentLocation { get; private set; }

    public EventCallback<OverlaySearchContext> UpdateSearch { get; init; }

    public SearchBox? SearchBox { get; init; }

    public async Task SetFilterAsync(IGameLocation location)
    {
        CurrentLocation = location;
        await ClearQueryAndFocusSearchBoxAsync();
        //! UpdateSearch not invoked because clearing query already results in an update
    }

    public async Task ClearLocationFilterAsync()
    {
        CurrentLocation = null;
        await UpdateSearch.InvokeAsync(this);
    }

    public async Task ClearQueryAndFocusSearchBoxAsync()
    {
        if (SearchBox is not null)
        {
            await SearchBox.SearchAsync(string.Empty);
            await SearchBox.FocusSearchBoxAsync();
        }
    }
}
