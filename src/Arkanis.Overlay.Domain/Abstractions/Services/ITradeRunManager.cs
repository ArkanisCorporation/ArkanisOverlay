namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Microsoft.Extensions.Primitives;
using Models.Trade;

public interface ITradeRunManager
{
    IChangeToken ChangeToken { get; }

    Task<int> GetInProgressCountAsync(CancellationToken cancellationToken = default);

    Task AddOrUpdateEntryAsync(TradeRun entry, CancellationToken cancellationToken = default);

    Task DeleteRunAsync(TradeRunId entryId, CancellationToken cancellationToken = default);

    Task<TradeRun?> GetRunAsync(TradeRunId runId, CancellationToken cancellationToken = default);

    Task<ICollection<TradeRun>> GetAllRunsAsync(CancellationToken cancellationToken = default);

    Task<ICollection<TradeRun>> GetInProgressRunsAsync(CancellationToken cancellationToken = default);
}
