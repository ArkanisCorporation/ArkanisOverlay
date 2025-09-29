#pragma warning disable CA1716
namespace Arkanis.Overlay.Components.Shared;
#pragma warning restore CA1716
using Dialogs;
using Domain.Abstractions.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

public sealed class UserConsentDialogProvider : ComponentBase, IUserConsentDialogService, IDisposable
{
    [Inject]
    public required IDialogService DialogService { get; set; }

    [Inject]
    public required IUserConsentDialogService.IConnector UserConsentDialogServiceConnector { get; set; }

    public void Dispose()
        => UserConsentDialogServiceConnector.Unlink(this);

    public async Task<IUserConsentDialogService.Result> RequestConsentAsync<T>(IDictionary<string, object> parameters)
        where T : ComponentBase, IUserConsentDialogService.IContent
    {
        var options = new UserConsentDialog.ContentOptions
        {
            ContentComponentType = typeof(T),
            ContentComponentParameters = parameters,
        };
        return await UserConsentDialog.ShowAsync(DialogService, options);
    }

    protected override async Task OnInitializedAsync()
        => await UserConsentDialogServiceConnector.LinkAsync(this);
}
