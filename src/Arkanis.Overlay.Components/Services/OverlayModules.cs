namespace Arkanis.Overlay.Components.Services;

using System.Diagnostics;
using Domain.Abstractions.Services;
using Domain.Models.Keyboard;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using MudBlazor;
using MudBlazor.FontIcons.MaterialSymbols;

public class OverlayModules(IOverlayControls overlayControls, IUserPreferencesManager preferencesManager)
{
    private readonly ICollection<Entry> _modules =
    [
        new UrlEntry
        {
            Url = "/search",
            Name = "Search",
            Description = "Find anything you're looking for.",
            Icon = Outlined.Search,
        },
        new UrlEntry
        {
            Url = "/hub",
            Name = "Hub",
            Description = "See what's going on around you.",
            Icon = Outlined.Hub,
            IsDisabled = true,
            IsInDevelopment = true,
        },
        new ActionEntry
        {
            Name = "Close",
            Description = "Close the Overlay.",
            Icon = Outlined.Close,
            Color = Color.Error,
            ShortcutOverride = preferencesManager.CurrentPreferences.InGameLaunchShortcut,
            Action = async (activationType, _) =>
            {
                // let the global keybindings close the overlay when invoked via hotkey
                if (activationType == ActivationType.Hotkey)
                {
                    return false;
                }

                await overlayControls.HideAsync().ConfigureAwait(false);
                return true;
            },
        },
        new UrlEntry
        {
            Url = "/inventory",
            Name = "Inventory",
            Description = "Track and manage your Inventory.",
            Icon = Icons.Material.Filled.Warehouse,
            GetChangeToken = serviceProvider => serviceProvider.GetRequiredService<IInventoryManager>().ChangeToken,
            GetUpdateCountAsync = async serviceProvider =>
            {
                var inventoryManager = serviceProvider.GetRequiredService<IInventoryManager>();
                return await inventoryManager.GetUnassignedCountAsync();
            },
        },
        new UrlEntry
        {
            Url = "/trade",
            Name = "Trade",
            Description = "Plan your next Haul.",
            Icon = Outlined.Storefront,
            GetChangeToken = serviceProvider => serviceProvider.GetRequiredService<ITradeRunManager>().ChangeToken,
            GetUpdateCountAsync = async serviceProvider =>
            {
                var inventoryManager = serviceProvider.GetRequiredService<ITradeRunManager>();
                return await inventoryManager.GetInProgressCountAsync();
            },
        },
        new UrlEntry
        {
            Url = "/mining",
            Name = "Mining",
            Description = "Manage your Mining Operations.",
            Icon = Outlined.Deblur,
            IsDisabled = true,
            IsInDevelopment = true,
        },
        new UrlEntry
        {
            Url = "/market",
            Name = "Market",
            Description = "Trade with other players.",
            Icon = Outlined.Store,
            IsDisabled = true,
            IsInDevelopment = true,
        },
        new UrlEntry
        {
            Url = "/hangar",
            Name = "Hangar",
            Description = "Manage your Fleet.",
            Icon = Outlined.GarageDoor,
        },
        new UrlEntry
        {
            Url = "/org",
            Name = "Org",
            Description = "Manage your Organization.",
            Icon = Icons.Material.Filled.Groups,
            IsDisabled = true,
            IsInDevelopment = true,
        },
        new UrlEntry
        {
            Url = "/settings",
            Name = "Settings",
            Description = "Configure the Overlay.",
            Icon = Outlined.Settings,
            ShortcutOverride = new KeyboardShortcut([KeyboardKey.F12]),
        },
    ];

    public ICollection<Entry> GetAll()
        => _modules;

    public enum ActivationType
    {
        Click,
        Hotkey,
    }

    [DebuggerDisplay("Entry {Name}")]
    public abstract class Entry
    {
        public required string Name { get; init; }

        public required string Description { get; init; }

        public Color Color { get; init; } = Color.Inherit;

        public bool IsDisabled { get; init; }
        public bool IsInDevelopment { get; init; }
        public string Icon { get; init; } = Icons.Material.Filled.ViewModule;
        public KeyboardShortcut? ShortcutOverride { get; init; }

        public Func<IServiceProvider, IChangeToken> GetChangeToken { get; set; } =
            _ => NullChangeToken.Singleton;

        public Func<IServiceProvider, ValueTask<int>> GetUpdateCountAsync { get; set; } =
            _ => ValueTask.FromResult(0);

        public virtual bool CanActivate(NavigationManager navigationManager, string currentUri)
            => !IsDisabled && !IsActive(navigationManager, currentUri);

        protected virtual bool Activate(NavigationManager navigationManager, string currentUri)
            => false;

        // Automatic invoke of Activate if not overriden to simplify usage of sync/async implementations.
        public virtual Task<bool> ActivateAsync(
            NavigationManager navigationManager,
            string currentUri,
            ActivationType activationType = ActivationType.Click,
            CancellationToken cancellationToken = default
        )
            => Task.FromResult(Activate(navigationManager, currentUri));

        public abstract bool IsActive(NavigationManager navigationManager, string currentUri);
    }

    [DebuggerDisplay("UrlEntry - {Name} ({Url})")]
    public class UrlEntry : Entry
    {
        public required string Url { get; init; }

        protected override bool Activate(NavigationManager navigationManager, string currentUri)
        {
            if (!CanActivate(navigationManager, currentUri))
            {
                return false;
            }

            var url = navigationManager.ToAbsoluteUri(Url).ToString();
            navigationManager.NavigateTo(url);
            return true;
        }

        public override bool IsActive(NavigationManager navigationManager, string currentUri)
            => currentUri.StartsWith(navigationManager.ToAbsoluteUri(Url).ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [DebuggerDisplay("ActionEntry - {Name} {Action}")]
    public class ActionEntry : Entry
    {
        public required Func<ActivationType, CancellationToken, Task<bool>> Action { get; init; }

        public override async Task<bool> ActivateAsync(
            NavigationManager navigationManager,
            string currentUri,
            ActivationType activationType = ActivationType.Click,
            CancellationToken cancellationToken = default
        )
        {
            if (!CanActivate(navigationManager, currentUri))
            {
                return false;
            }

            return await Action(activationType, cancellationToken);
        }

        public override bool IsActive(NavigationManager navigationManager, string currentUri)
            => false;
    }
}
