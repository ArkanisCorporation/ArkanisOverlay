namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Microsoft.Extensions.Primitives;
using Models.Trade;

public interface ITradeRunManager
{
    public IChangeToken ChangeToken { get; }

    public Task<int> GetInProgressCountAsync(CancellationToken cancellationToken = default);

    public Task AddOrUpdateEntryAsync(TradeRun entry, CancellationToken cancellationToken = default);

    public Task DeleteRunAsync(TradeRunId entryId, CancellationToken cancellationToken = default);

    public Task<TradeRun?> GetRunAsync(TradeRunId runId, CancellationToken cancellationToken = default);

    public Task<ICollection<TradeRun>> GetAllRunsAsync(CancellationToken cancellationToken = default);

    public Task<ICollection<TradeRun>> GetInProgressRunsAsync(CancellationToken cancellationToken = default);
}
