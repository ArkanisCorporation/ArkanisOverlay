namespace Arkanis.Overlay.Infrastructure.Repositories.Local.Specialised;

using Domain.Abstractions;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using Services;

public class GamePurchasePricingRepositoryAggregate(
    ServiceDependencyResolver resolver,
    IEnumerable<IGamePurchasePricingRepository> repositories
) : IDependable
{
    public bool IsReady { get; private set; }

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await resolver.DependsOn(this, repositories)
            .WaitUntilReadyAsync(cancellationToken)
            .ContinueWith(_ => IsReady = true, cancellationToken);

    public async ValueTask<ICollection<IGameEntityPurchasePrice>> GetAllForAsync(IDomainId domainId, CancellationToken cancellationToken = default)
    {
        var result = new List<IGameEntityPurchasePrice>();
        foreach (var repository in repositories)
        {
            var items = await repository.GetPurchasePricesForAsync(domainId, cancellationToken);
            result.AddRange(items);
        }

        return result;
    }
}

public class GameSalePricingRepositoryAggregate(
    ServiceDependencyResolver resolver,
    IEnumerable<IGameSalePricingRepository> repositories
) : IDependable
{
    public bool IsReady { get; private set; }

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await resolver.DependsOn(this, repositories)
            .WaitUntilReadyAsync(cancellationToken)
            .ContinueWith(_ => IsReady = true, cancellationToken);

    public async ValueTask<ICollection<IGameEntitySalePrice>> GetAllForAsync(IDomainId domainId, CancellationToken cancellationToken = default)
    {
        var result = new List<IGameEntitySalePrice>();
        foreach (var repository in repositories)
        {
            var items = await repository.GetSalePricesForAsync(domainId, cancellationToken);
            result.AddRange(items);
        }

        return result;
    }
}

public class GameRentalPricingRepositoryAggregate(
    ServiceDependencyResolver resolver,
    IEnumerable<IGameRentalPricingRepository> repositories
) : IDependable
{
    public bool IsReady { get; private set; }

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await resolver.DependsOn(this, repositories)
            .WaitUntilReadyAsync(cancellationToken)
            .ContinueWith(_ => IsReady = true, cancellationToken);

    public async ValueTask<ICollection<IGameEntityRentalPrice>> GetAllForAsync(IDomainId domainId, CancellationToken cancellationToken = default)
    {
        var result = new List<IGameEntityRentalPrice>();
        foreach (var repository in repositories)
        {
            var items = await repository.GetRentalPricesForAsync(domainId, cancellationToken);
            result.AddRange(items);
        }

        return result;
    }
}

public class GameMarketPricingRepositoryAggregate(
    ServiceDependencyResolver resolver,
    IEnumerable<IGameMarketPricingRepository> repositories
) : IDependable
{
    public bool IsReady { get; private set; }

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await resolver.DependsOn(this, repositories)
            .WaitUntilReadyAsync(cancellationToken)
            .ContinueWith(_ => IsReady = true, cancellationToken);

    public async ValueTask<ICollection<GameEntityMarketPrice>> GetAllForAsync(IDomainId domainId, CancellationToken cancellationToken = default)
    {
        var result = new List<GameEntityMarketPrice>();
        foreach (var repository in repositories)
        {
            var items = await repository.GetMarketPricesForAsync(domainId, cancellationToken);
            result.AddRange(items);
        }

        return result;
    }
}
