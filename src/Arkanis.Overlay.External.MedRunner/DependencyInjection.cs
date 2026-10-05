namespace Arkanis.Overlay.External.MedRunner;

using API;
using API.Abstractions;
using API.Endpoints;
using API.Mocks;
using Common.Extensions;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    private static IServiceCollection AddCommonMedRunnerApiServices(this IServiceCollection services)
        => services
            .AddSingleton<MedRunnerAuthenticator>()
            .Alias<ExternalAuthenticator, MedRunnerAuthenticator>()
            .AddSingleton<IMedRunnerTokenProvider, ApiKeySourcedTokenProvider>()
            .AddSingleton<IMedRunnerApiClient, MedRunnerApiClient>();

    public static IServiceCollection AddMockMedRunnerApiClient(
        this IServiceCollection services,
        Func<IServiceProvider, IMedRunnerClientConfig>? createOptions = null
    )
        => services
            .AddCommonMedRunnerApiServices()
            .AddMockMedRunnerApiEndpoints()
            .AddSingleton(serviceProvider =>
                {
                    var config = createOptions?.Invoke(serviceProvider) ?? new MedRunnerClientConfig();
                    config.IsMock = true;
                    return config;
                }
            );

    public static IServiceCollection AddLiveMedRunnerApiClient(
        this IServiceCollection services,
        Func<IServiceProvider, IMedRunnerClientConfig>? createOptions = null
    )
        => services
            .AddCommonMedRunnerApiServices()
            .AddLiveMedRunnerApiEndpoints()
            .AddSingleton(serviceProvider =>
                {
                    var config = createOptions?.Invoke(serviceProvider) ?? new MedRunnerClientConfig();
                    config.IsMock = false;
                    return config;
                }
            );
}
