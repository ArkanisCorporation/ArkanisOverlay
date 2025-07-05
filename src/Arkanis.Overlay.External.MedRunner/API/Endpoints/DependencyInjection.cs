namespace Arkanis.Overlay.External.MedRunner.API.Endpoints;

using Abstractions.Endpoints;
using Auth;
using ChatMessage;
using Client;
using Code;
using Common.Extensions;
using Emergency;
using Microsoft.Extensions.DependencyInjection;
using OrgSettings;
using Staff;
using WebSocket;

public static class DependencyInjection
{
    internal static IServiceCollection AddLiveMedRunnerApiEndpoints(this IServiceCollection services)
    {
        // Register concrete endpoint implementations
        services
            .AddSingleton<AuthEndpoint>()
            .AddSingleton<ChatMessageEndpoint>()
            .AddSingleton<ClientEndpoint>()
            .AddSingleton<CodeEndpoint>()
            .AddSingleton<EmergencyEndpoint>()
            .AddSingleton<OrgSettingsEndpoint>()
            .AddSingleton<StaffEndpoint>()
            .AddSingleton<WebSocketEndpoint>();

        // Register interface aliases
        services
            .Alias<IAuthEndpoint, AuthEndpoint>()
            .Alias<IChatMessageEndpoint, ChatMessageEndpoint>()
            .Alias<IClientEndpoint, ClientEndpoint>()
            .Alias<ICodeEndpoint, CodeEndpoint>()
            .Alias<IEmergencyEndpoint, EmergencyEndpoint>()
            .Alias<IOrgSettingsEndpoint, OrgSettingsEndpoint>()
            .Alias<IStaffEndpoint, StaffEndpoint>()
            .Alias<IWebSocketEndpoint, WebSocketEndpoint>();

        return services;
    }
}
