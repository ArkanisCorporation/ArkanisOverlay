namespace Arkanis.Overlay.UI.ViewModels.Pages;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Metadata;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Search;
using JetBrains.Annotations;
using ReactiveUI;
using Utils;

public class SearchPageViewModel(IScreen screen, ISearchService searchService) : PageViewModelBase(screen)
{
    [PrivateApi]
    [UsedImplicitly]
    public SearchPageViewModel() : this(FakeScreen.Instance, new StaticResultSearchService())
    {
    }

    public override string UrlPathSegment
        => "search";

    [field: MaybeNull]
    public ReactiveCommand<string, GameEntitySearchResults> SearchCommand
        => field ??= ReactiveCommand.CreateFromTask<string, GameEntitySearchResults>(ExecuteSearchAsync);

    public string QueryText { get; set; } = string.Empty;
    public GameEntitySearchResults Results { get; private set; } = StaticResultSearchService.DefaultResults;

    public Exception? Error { get; private set; }

    protected override void InitBindings(CompositeDisposable disposable)
    {
        SearchCommand.ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(results => Results = results)
            .DisposeWith(disposable);

        SearchCommand.ThrownExceptions
            .Subscribe(ex => Error = ex)
            .DisposeWith(disposable);

        this.WhenAnyValue(x => x.QueryText)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .DistinctUntilChanged()
            .InvokeCommand(SearchCommand)
            .DisposeWith(disposable);
    }

    private async Task<GameEntitySearchResults> ExecuteSearchAsync(string query, CancellationToken cancellationToken)
    {
        var queries = new List<SearchQuery>
        {
            TextSearch.Fuzzy(query),
        };
        return await searchService.SearchAsync(queries, cancellationToken);
    }
}
