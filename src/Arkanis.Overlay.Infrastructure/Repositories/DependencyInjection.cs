namespace Arkanis.Overlay.Infrastructure.Repositories;

using Domain.Models.Game;
using Local;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Hosted;
using Sync;

public static class DependencyInjection
{
    public static IServiceCollection AddUexInMemoryGameEntityServices(this IServiceCollection services)
    {
        services
            .AddUexInMemoryRepositories()
            .AddUexSyncRepositoryServices();

        var hostedServiceType = typeof(InitializeGameEntityRepositoryHostedService<>);
        foreach (var gameEntityType in GameEntityConstants.GameEntityTypes)
        {
            var serviceType = hostedServiceType.MakeGenericType(gameEntityType);
            services.AddSingleton(typeof(IHostedService), serviceType);
        }

        return services;
    }
}
