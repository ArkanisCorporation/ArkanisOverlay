namespace Arkanis.Overlay.External.MedRunner;

using API;
using API.Abstractions;
using API.Abstractions.Endpoints;
using API.Endpoints.Auth;
using API.Endpoints.ChatMessage;
using API.Endpoints.Client;
using API.Endpoints.Code;
using API.Endpoints.Emergency;
using API.Endpoints.OrgSettings;
using API.Endpoints.Staff;
using API.Endpoints.WebSocket;
using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddLiveMedRunnerApiClient(this IServiceCollection services, Func<IServiceProvider, IMedRunnerClientConfig>? createOptions = null)
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
            .Alias<IWebsocketEndpoint, WebSocketEndpoint>();

        return services
            .AddSingleton(createOptions ?? (_ => new MedRunnerClientConfig()))
            .AddSingleton<IMedRunnerTokenProvider, ApiKeySourcedTokenProvider>()
            .AddSingleton<IMedRunnerApiClient, MedRunnerApiClient>();
    }
}
