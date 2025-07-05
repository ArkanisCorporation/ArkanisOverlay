namespace Arkanis.Overlay.External.MedRunner.API.Mocks;

using Abstractions;
using Abstractions.Endpoints;
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
            .AddSingleton<IAuthEndpoint, MockAuthEndpoint>()
            .AddSingleton<IChatMessageEndpoint, MockChatMessageEndpoint>()
            .AddSingleton<IClientEndpoint, MockClientEndpoint>()
            .AddSingleton<ICodeEndpoint, MockCodeEndpoint>()
            .AddSingleton<IEmergencyEndpoint, MockEmergencyEndpoint>()
            .AddSingleton<IOrgSettingsEndpoint, MockOrgSettingsEndpoint>()
            .AddSingleton<IStaffEndpoint, MockStaffEndpoint>()
            .AddSingleton<IWebSocketEndpoint, MockWebSocketEndpoint>()
            .AddSingleton<IWebSocketEventProvider, MockWebSocketEventProvider>();

        return services;
    }
}
