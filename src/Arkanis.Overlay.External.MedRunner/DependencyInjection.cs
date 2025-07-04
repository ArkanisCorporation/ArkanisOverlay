namespace Arkanis.Overlay.External.MedRunner;

using API;
using API.Abstractions;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddMedRunnerApiClient(this IServiceCollection services, Func<IServiceProvider, ApiConfig>? createOptions = null)
        => services
            .AddSingleton(createOptions ?? (_ => new ApiConfig()))
            .AddSingleton<IMedRunnerTokenProvider, ApiKeySourcedTokenProvider>()
            .AddSingleton<IMedRunnerApiClient, MedRunnerApiClient>();
}
