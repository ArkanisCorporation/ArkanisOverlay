namespace Arkanis.Overlay.External.MedRunner.API;

using Abstractions;

/// <inheritdoc cref="IMedRunnerClientConfig" />
public class MedRunnerClientConfig : IMedRunnerClientConfig
{
    /// <inheritdoc />
    public string BaseUrl { get; set; } = "https://api.medrunner.space";

    /// <inheritdoc />
    public string? AccessToken { get; set; }

    /// <inheritdoc />
    public string? RefreshToken { get; set; }

    /// <inheritdoc />
    public bool CookieAuth { get; set; }

    /// <inheritdoc />
    public bool IsMock { get; set; }
}
