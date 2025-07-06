namespace Arkanis.Overlay.External.MedRunner;

using API;
using API.Abstractions;
using API.Endpoints;
using API.Mocks;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddMockMedRunnerApiClient(
        this IServiceCollection services,
        Func<IServiceProvider, IMedRunnerClientConfig>? createOptions = null
    )
        => services
            .AddSingleton(createOptions ?? (_ => new MedRunnerClientConfig()))
            .AddMockMedRunnerApiEndpoints()
            .AddSingleton<IMedRunnerTokenProvider, ApiKeySourcedTokenProvider>()
            .AddSingleton<IMedRunnerApiClient, MedRunnerApiClient>();

    public static IServiceCollection AddLiveMedRunnerApiClient(
        this IServiceCollection services,
        Func<IServiceProvider, IMedRunnerClientConfig>? createOptions = null
    )
        => services
            .AddSingleton(createOptions ?? (_ => new MedRunnerClientConfig()))
            .AddLiveMedRunnerApiEndpoints()
            .AddSingleton<IMedRunnerTokenProvider, ApiKeySourcedTokenProvider>()
            .AddSingleton<IMedRunnerApiClient, MedRunnerApiClient>();
}
