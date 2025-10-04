namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Microsoft.AspNetCore.Components;

public interface IUserConsentDialogService
{
    public Task<Result> RequestConsentAsync<T>(IDictionary<string, object> parameters) where T : ComponentBase, IContent;

    public interface IConnector
    {
        public Task LinkAsync(IUserConsentDialogService connector);
        public void Unlink(IUserConsentDialogService connector);
    }

    public record Result(bool WasAccepted)
    {
        public static readonly Result Consent = new(true);
        public static readonly Result Reject = new(false);
    }

    public record Context(bool CanConsent = false);

    public interface IContent
    {
        public Context DialogContext { get; set; }

        public EventCallback<Context> DialogContextChanged { get; set; }
    }
}
