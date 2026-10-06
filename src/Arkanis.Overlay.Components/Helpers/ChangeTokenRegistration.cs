namespace Arkanis.Overlay.Components.Helpers;

using Microsoft.Extensions.Primitives;

public sealed class ChangeTokenRegistration : IDisposable
{
    public delegate void ChangeCallback();

    private readonly Func<IChangeToken> _getChangeToken;
    private IDisposable? _currentRegistration;

    public ChangeTokenRegistration(Func<IChangeToken> getChangeToken)
    {
        _getChangeToken = getChangeToken;
        UpdateRegistration();
    }

    public void Dispose()
        => _currentRegistration?.Dispose();

    public event ChangeCallback? OnChange;

    private void ChangeHandler(object? _)
        => UpdateRegistration();

    private void UpdateRegistration()
    {
        _currentRegistration?.Dispose();

        try
        {
            var newChangeToken = _getChangeToken();
            if (!newChangeToken.HasChanged)
            {
                // the new token still indicates a change, do not update the registration to prevent stack overflow
                _currentRegistration = newChangeToken.RegisterChangeCallback(ChangeHandler, null);
            }

            OnChange?.Invoke();
        }
        catch (ObjectDisposedException)
        {
            // ignore
        }
    }
}
