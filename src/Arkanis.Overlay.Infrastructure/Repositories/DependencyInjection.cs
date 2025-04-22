namespace Arkanis.Overlay.Infrastructure.Repositories;

using Local;
using Microsoft.Extensions.DependencyInjection;
using Sync;

public static class DependencyInjection
{
    internal static IServiceCollection AddUexInMemoryGameEntityServices(this IServiceCollection services)
        => services
            .AddUexInMemoryRepositories()
            .AddUexSyncRepositoryServices();
}
