namespace Arkanis.Overlay.External.MedRunner.API.Mocks;

using Abstractions;
using Abstractions.Endpoints;
using Common.Extensions;
using Endpoints;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    internal static IServiceCollection AddMockMedRunnerApiEndpoints(this IServiceCollection services)
    {
        // Register mock endpoints
        services
            .AddSingleton<MockAuthEndpoint>()
            .AddSingleton<MockChatMessageEndpoint>()
            .AddSingleton<MockClientEndpoint>()
            .AddSingleton<MockCodeEndpoint>()
            .AddSingleton<MockEmergencyEndpoint>()
            .AddSingleton<MockOrgSettingsEndpoint>()
            .AddSingleton<MockStaffEndpoint>()
            .AddSingleton<MockWebSocketEndpoint>()
            .AddSingleton<MockWebSocketEventProvider>();

        // Register mock endpoints
        services
            .Alias<IAuthEndpoint, MockAuthEndpoint>()
            .Alias<IChatMessageEndpoint, MockChatMessageEndpoint>()
            .Alias<IClientEndpoint, MockClientEndpoint>()
            .Alias<ICodeEndpoint, MockCodeEndpoint>()
            .Alias<IEmergencyEndpoint, MockEmergencyEndpoint>()
            .Alias<IOrgSettingsEndpoint, MockOrgSettingsEndpoint>()
            .Alias<IStaffEndpoint, MockStaffEndpoint>()
            .Alias<IWebSocketEndpoint, MockWebSocketEndpoint>()
            .Alias<IWebSocketEventProvider, MockWebSocketEventProvider>();

        return services;
    }
}
