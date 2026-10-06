namespace Arkanis.Overlay.Infrastructure.UnitTests.Services.MedRunner;

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Arkanis.Overlay.External.MedRunner.API;
using Arkanis.Overlay.External.MedRunner.API.Abstractions;
using Arkanis.Overlay.External.MedRunner.API.Abstractions.Endpoints;
using Arkanis.Overlay.External.MedRunner.API.Endpoints;
using Arkanis.Overlay.External.MedRunner.API.Endpoints.Auth.Request;
using Arkanis.Overlay.External.MedRunner.API.Endpoints.ChatMessage.Request;
using Arkanis.Overlay.External.MedRunner.API.Endpoints.Emergency.Request;
using Arkanis.Overlay.External.MedRunner.API.Endpoints.WebSocket;
using Arkanis.Overlay.External.MedRunner.Models;
using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shouldly;

public sealed class MedRunnerAuthenticationUnitTests
{
    private const string LiveChatMessageJson = """
        {"emergencyId":"alert","senderId":"client","messageSentTimestamp":"2026-10-06T16:39:03.7099209Z",
         "contents":"THIS ALERT IS AN INTERNAL IT TEST!","edited":false,"deleted":false,
         "updated":"2026-10-06T16:39:03.7099259Z","id":"chat-message","created":"2026-10-06T16:39:03.7099208Z"}
        """;

    [Fact]
    public async Task HttpChatResponsePreservesIsoTimestampAndContentsFromTheLiveApi()
    {
        using var cache = new EphemeralMemoryCache();
        using var httpClient = new HttpClient(new StaticResponseHandler(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(LiveChatMessageJson),
        }));
        var endpoint = new TestApiEndpoint(new MedRunnerClientConfig(), new StaticTokenProvider("access-token"), cache,
            new RecordingLogger<TestApiEndpoint>(), httpClient);

        var response = await endpoint.SendChatMessageAsync();

        response.Success.ShouldBeTrue(response.ErrorMessage);
        AssertLiveChatMessage(response.Data);
    }

    [Fact]
    public async Task HttpChatHistoryPreservesIsoTimestampAndContentsFromTheLiveApi()
    {
        using var cache = new EphemeralMemoryCache();
        using var httpClient = new HttpClient(new StaticResponseHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"{{\"data\":[{LiveChatMessageJson}],\"paginationToken\":null}}"),
        }));
        var endpoint = new TestApiEndpoint(new MedRunnerClientConfig(), new StaticTokenProvider("access-token"), cache,
            new RecordingLogger<TestApiEndpoint>(), httpClient);

        var response = await endpoint.GetChatHistoryAsync();

        response.Success.ShouldBeTrue(response.ErrorMessage);
        AssertLiveChatMessage(response.Data.Data.ShouldHaveSingleItem());
    }

    [Theory]
    [InlineData("ChatMessageCreate")]
    [InlineData("ChatMessageUpdate")]
    public void RealTimeChatPreservesIsoTimestampAndContentsFromTheLiveApi(string eventName)
    {
        var logger = new RecordingLogger<SignalRMessageHandler>();
        var handler = new SignalRMessageHandler(logger);
        var received = new List<ChatMessage>();
        handler.ChatMessageCreated += (_, message) => received.Add(message);
        handler.ChatMessageUpdated += (_, message) => received.Add(message);
        using var json = JsonDocument.Parse(LiveChatMessageJson);

        handler.HandleMessage(eventName, json.RootElement);

        AssertLiveChatMessage(received.ShouldHaveSingleItem());
        logger.LogLevels.ShouldNotContain(LogLevel.Error);
    }

    private static void AssertLiveChatMessage(ChatMessage message)
    {
        message.Id.ShouldBe("chat-message");
        message.EmergencyId.ShouldBe("alert");
        message.SenderId.ShouldBe("client");
        message.Content.ShouldBe("THIS ALERT IS AN INTERNAL IT TEST!");
        message.SentAt.ShouldBe(new DateTimeOffset(2026, 10, 6, 16, 39, 3, TimeSpan.Zero).AddTicks(7_099_209));
    }

    [Fact]
    public async Task CandidateApiTokenIsStagedWithoutChangingTheActiveClientConfiguration()
    {
        using var cache = new EphemeralMemoryCache();
        var config = new MedRunnerClientConfig();
        var serviceProvider = new DeferredServiceProvider();
        using var tokenProvider = new ApiKeySourcedTokenProvider(serviceProvider, config, cache, new RecordingLogger<ApiKeySourcedTokenProvider>());
        serviceProvider.Service = new StaticApiClient(tokenProvider, new SuccessfulAuthEndpoint(CreateValidGrant()));

        var result = await tokenProvider.AuthenticateApiTokenAsync("candidate-api-token", CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Identity.Name.ShouldBe("KronnY");
        config.AccessToken.ShouldBeNull();
        config.RefreshToken.ShouldBeNull();
        tokenProvider.IsAuthenticated.ShouldBeFalse();
    }

    [Fact]
    public async Task MissingAccessTokenDoesNotLogValidationWarning()
    {
        using var cache = new EphemeralMemoryCache();
        var logger = new RecordingLogger<ApiKeySourcedTokenProvider>();
        using var tokenProvider = new ApiKeySourcedTokenProvider(new EmptyServiceProvider(), new MedRunnerClientConfig(), cache, logger);

        var accessToken = await tokenProvider.GetAccessTokenAsync();

        accessToken.ShouldBeNull();
        logger.LogLevels.ShouldNotContain(LogLevel.Warning);
    }

    [Fact]
    public async Task AuthenticatedRequestUsesReturnedAccessToken()
    {
        using var cache = new EphemeralMemoryCache();
        var endpoint = new TestApiEndpoint(
            new MedRunnerClientConfig(),
            new StaticTokenProvider("access-token"),
            cache,
            new RecordingLogger<TestApiEndpoint>()
        );

        using var request = await endpoint.CreateAuthenticatedRequestAsync();

        request.Headers.Authorization.ShouldNotBeNull();
        request.Headers.Authorization.Scheme.ShouldBe("Bearer");
        request.Headers.Authorization.Parameter.ShouldBe("access-token");
    }

    [Fact]
    public async Task UnauthenticatedGetDoesNotRequireATokenOrSendRequestOptionsAsJson()
    {
        using var cache = new EphemeralMemoryCache();
        HttpRequestMessage? sentRequest = null;
        var endpoint = new TestApiEndpoint(
            new MedRunnerClientConfig(),
            new StaticTokenProvider(null),
            cache,
            new RecordingLogger<TestApiEndpoint>(),
            new HttpClient(new StaticResponseHandler(request =>
            {
                sentRequest = request;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("\"connected\""),
                };
            }))
        );

        var response = await endpoint.GetUnauthenticatedAsync();

        response.Success.ShouldBeTrue();
        sentRequest.ShouldNotBeNull();
        sentRequest.Headers.Authorization.ShouldBeNull();
        sentRequest.Content.ShouldBeNull();
    }

    [Fact]
    public async Task EmptyHttpErrorProvidesAUsefulMessage()
    {
        using var cache = new EphemeralMemoryCache();
        var endpoint = new TestApiEndpoint(
            new MedRunnerClientConfig(),
            new StaticTokenProvider("access-token"),
            cache,
            new RecordingLogger<TestApiEndpoint>(),
            new HttpClient(new StaticResponseHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)))
        );

        var response = await endpoint.GetAuthenticatedAsync();

        response.Success.ShouldBeFalse();
        response.ErrorMessage.ShouldBe("Medrunner API request failed with status 401 (Unauthorized).");
    }

    private sealed class TestApiEndpoint(
        IMedRunnerClientConfig config,
        IMedRunnerTokenProvider tokenProvider,
        IMemoryCache cache,
        ILogger<TestApiEndpoint> logger,
        HttpClient? httpClient = null,
        string endpointName = "test"
    ) : ApiEndpoint(config, tokenProvider, cache, logger, httpClient)
    {
        protected override string Endpoint
            => endpointName;

        public Task<HttpRequestMessage> CreateAuthenticatedRequestAsync()
            => CreateRequestMessageAsync(HttpMethod.Get, "https://api.medrunner.space/test");

        public Task<HttpRequestMessage> CreateEmergencyRequestAsync(CreateEmergencyRequest body)
            => CreateRequestMessageAsync(HttpMethod.Post, "https://api.medrunner.space/emergency/", body);

        public Task<ApiResponse<string>> GetUnauthenticatedAsync()
            => GetRequestAsync<string>("", requestOptions: RequestOptions.Unauthenticated);

        public Task<ApiResponse<string>> GetAuthenticatedAsync()
            => GetRequestAsync<string>("");

        public Task<ApiResponse<Team>> GetTeamAsync()
            => GetRequestAsync<Team>("/team");

        public Task<ApiResponse<List<Emergency>>> GetEmergenciesAsync()
            => GetRequestAsync<List<Emergency>>("/emergencies");

        public Task<ApiResponse<string>> GetFreshAsync()
            => GetRequestAsync<string>("", requestOptions: new RequestOptions { CacheDuration = TimeSpan.Zero });

        public Task<ApiResponse<ChatMessage>> SendChatMessageAsync()
            => PostRequestAsync<ChatMessage>("/chatMessage", new ChatMessageRequest { EmergencyId = "alert", Contents = "THIS ALERT IS AN INTERNAL IT TEST!" });

        public Task<ApiResponse<ApiPaginatedResponse<ChatMessage>>> GetChatHistoryAsync()
            => GetRequestAsync<ApiPaginatedResponse<ChatMessage>>("/chatMessage/conversation/alert");
    }

    [Fact]
    public async Task CachedClientResponsesAreIsolatedBetweenAccounts()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var httpClient = new HttpClient(new StaticResponseHandler(request => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"\"{request.Headers.Authorization!.Parameter}\""),
        }));
        var logger = new RecordingLogger<TestApiEndpoint>();
        var first = new TestApiEndpoint(new MedRunnerClientConfig(), new StaticTokenProvider("first-client"), cache, logger, httpClient);
        var second = new TestApiEndpoint(new MedRunnerClientConfig(), new StaticTokenProvider("second-client"), cache, logger, httpClient);

        (await first.GetAuthenticatedAsync()).Data.ShouldBe("first-client");
        (await second.GetAuthenticatedAsync()).Data.ShouldBe("second-client");
    }

    [Fact]
    public async Task RefreshCanBypassCachedResponsesToDiscoverNewAlerts()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var requests = 0;
        using var httpClient = new HttpClient(new StaticResponseHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"\"response-{++requests}\""),
        }));
        var endpoint = new TestApiEndpoint(new MedRunnerClientConfig(), new StaticTokenProvider("client"), cache,
            new RecordingLogger<TestApiEndpoint>(), httpClient);
        (await endpoint.GetAuthenticatedAsync()).Data.ShouldBe("response-1");
        (await endpoint.GetFreshAsync()).Data.ShouldBe("response-2");
    }

    [Theory]
    [InlineData("{\"id\":\"team-1\",\"name\":\"Rescue\"}", true)]
    [InlineData("{\"unexpected\":\"shape\"}", false)]
    public async Task IncomingJsonIsLoggedEvenWhenRequiredTeamPropertiesAreMissing(string json, bool success)
    {
        using var cache = new EphemeralMemoryCache();
        var logger = new RecordingLogger<TestApiEndpoint>();
        using var httpClient = new HttpClient(new StaticResponseHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json),
        }));
        var endpoint = new TestApiEndpoint(new MedRunnerClientConfig(), new StaticTokenProvider("access-token"), cache, logger, httpClient);

        var response = await endpoint.GetTeamAsync();

        response.Success.ShouldBe(success);
        logger.Messages.ShouldContain(message => message.Contains(json, StringComparison.Ordinal)
            && message.Contains("GET", StringComparison.Ordinal) && message.Contains("/team", StringComparison.Ordinal));
        logger.Messages.ShouldNotContain(message => message.Contains("access-token", StringComparison.Ordinal));
        if (!success)
        {
            logger.Entries.ShouldContain(entry => entry.Level == LogLevel.Error && entry.Message.Contains(json, StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task AuthenticationResponseTokensAreOmittedFromJsonAndDecodingErrorLogs()
    {
        using var cache = new EphemeralMemoryCache();
        var logger = new RecordingLogger<TestApiEndpoint>();
        using var httpClient = new HttpClient(new StaticResponseHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"accessToken\":\"secret-access\",\"refreshToken\":\"secret-refresh\"}"),
        }));
        var endpoint = new TestApiEndpoint(new MedRunnerClientConfig(), new StaticTokenProvider("access-token"), cache, logger, httpClient, "auth");

        var response = await endpoint.GetTeamAsync();

        response.Success.ShouldBeFalse();
        logger.Messages.ShouldContain(message => message.Contains("authentication response omitted", StringComparison.Ordinal));
        logger.Messages.ShouldNotContain(message => message.Contains("secret-access", StringComparison.Ordinal) || message.Contains("secret-refresh", StringComparison.Ordinal));
    }

    [Fact]
    public void RealTimeJsonIsLoggedBeforeDecodingAndAMalformedTeamDoesNotPreventTheNextUpdate()
    {
        var logger = new RecordingLogger<SignalRMessageHandler>();
        var handler = new SignalRMessageHandler(logger);
        var teams = new List<Team>();
        handler.TeamUpdated += (_, team) => teams.Add(team);
        using var malformed = JsonDocument.Parse("{\"unexpected\":\"shape\"}");
        using var valid = JsonDocument.Parse("{\"id\":\"team-1\",\"name\":\"Rescue\"}");

        handler.HandleMessage("TeamUpdate", malformed.RootElement);
        handler.HandleMessage("TeamUpdate", valid.RootElement);

        teams.ShouldHaveSingleItem().Name.ShouldBe("Rescue");
        logger.Entries.ShouldContain(entry => entry.Level == LogLevel.Error
            && entry.Message.Contains("TeamUpdate", StringComparison.Ordinal)
            && entry.Message.Contains("{\"unexpected\":\"shape\"}", StringComparison.Ordinal));
        logger.Messages.ShouldContain(message => message.Contains("{\"id\":\"team-1\",\"name\":\"Rescue\"}", StringComparison.Ordinal));
    }

    [Fact]
    public async Task UnassignedResponseRosterAndMillisecondTimestampsFromTheLiveApiCanBeLoaded()
    {
        const string json = """
            [{"system":"Stanton","subsystem":"Hurston","tertiaryLocation":"Aberdeen","threatLevel":2,
              "remarks":"Closest location: Stanton / Hurston / Aberdeen / Klescher Rehabilitation Facility",
              "clientRsiHandle":"Pilot","clientId":"client","subscriptionTier":"None","status":1,"cancellationReason":0,
              "coordinationThread":{"id":"message","channelId":"channel"},
              "respondingTeam":{"maxMembers":6,"staff":[],"dispatchers":[],"allMembers":[]},"respondingTeams":[],
              "creationTimestamp":1791302630769,"acceptedTimestamp":1791302634878,"completionTimestamp":1791302640000,
              "rating":0,"test":false,"origin":1,
              "clientData":{"rsiHandle":"Pilot","rsiProfileLink":"https://robertsspaceindustries.com/citizens/Pilot",
                            "gotClientData":true,"redactedOrgOnProfile":false,"reported":false,"userSid":"sid"},
              "missionName":"Lucky Criticism","submissionSource":1,"isComplete":false,
              "updated":"2026-10-06T16:03:54.878+00:00","id":"alert","created":"2026-10-06T16:03:50.768+00:00"}]
            """;
        using var cache = new EphemeralMemoryCache();
        using var httpClient = new HttpClient(new StaticResponseHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json),
        }));
        var endpoint = new TestApiEndpoint(new MedRunnerClientConfig(), new StaticTokenProvider("access-token"), cache,
            new RecordingLogger<TestApiEndpoint>(), httpClient);

        var response = await endpoint.GetEmergenciesAsync();

        response.Success.ShouldBeTrue(response.ErrorMessage);
        var emergency = response.Data.ShouldHaveSingleItem();
        emergency.Id.ShouldBe("alert");
        emergency.RespondingTeam.MaxMembers.ShouldBe(6);
        emergency.RespondingTeam.Staff.ShouldBeEmpty();
        emergency.CreatedAt.ShouldBe(new DateTimeOffset(2026, 10, 6, 16, 3, 50, TimeSpan.Zero).AddMilliseconds(769));
        emergency.AcceptedAt.ShouldBe(new DateTimeOffset(2026, 10, 6, 16, 3, 54, TimeSpan.Zero).AddMilliseconds(878));
        emergency.CompletedAt.ShouldBe(new DateTimeOffset(2026, 10, 6, 16, 4, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task EmergencyRequestUsesCamelCaseAndOmitsUnsetLocationFields()
    {
        using var cache = new EphemeralMemoryCache();
        var endpoint = new TestApiEndpoint(new MedRunnerClientConfig(), new StaticTokenProvider("access-token"), cache,
            new RecordingLogger<TestApiEndpoint>());
        using var request = await endpoint.CreateEmergencyRequestAsync(new CreateEmergencyRequest
        {
            Location = new Location { System = "Stanton", Subsystem = "Crusader" },
            ThreatLevel = ThreatLevel.Low,
            Remarks = "Closest location: Stanton / Crusader / Orison",
        });

        request.Content.ShouldNotBeNull();
        request.Content.Headers.ContentType!.MediaType.ShouldBe("application/json");
        using var json = JsonDocument.Parse(await request.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        json.RootElement.TryGetProperty("location", out var location).ShouldBeTrue();
        location.GetProperty("system").GetString().ShouldBe("Stanton");
        location.GetProperty("subsystem").GetString().ShouldBe("Crusader");
        location.TryGetProperty("tertiaryLocation", out _).ShouldBeFalse();
        json.RootElement.TryGetProperty("rsiHandle", out _).ShouldBeFalse();
        json.RootElement.GetProperty("remarks").GetString().ShouldBe("Closest location: Stanton / Crusader / Orison");
    }

    private sealed class StaticTokenProvider(string? accessToken) : IMedRunnerTokenProvider
    {
        public ClaimsIdentity? Identity
            => null;

        [MemberNotNullWhen(true, nameof(Identity))]
        public bool IsAuthenticated
            => false;

        public Task<string?> GetAccessTokenAsync()
            => Task.FromResult<string?>(accessToken);

        public Task<string?> GetAccessTokenAsync(string source)
            => Task.FromResult<string?>(accessToken);

        public Task<Result<MedRunnerTokenAuthentication>> AuthenticateApiTokenAsync(string apiToken, CancellationToken cancellationToken)
            => Task.FromResult(Result.Fail<MedRunnerTokenAuthentication>("Not used by this test."));

        public void ApplyAuthentication(MedRunnerTokenAuthentication authentication)
        {
        }

        public void ClearAuthentication()
        {
        }
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType)
            => null;
    }

    private sealed class DeferredServiceProvider : IServiceProvider
    {
        public object? Service { get; set; }

        public object? GetService(Type serviceType)
            => serviceType == typeof(IMedRunnerApiClient) ? Service : null;
    }

    private sealed class StaticApiClient(IMedRunnerTokenProvider tokenProvider, IAuthEndpoint authEndpoint) : IMedRunnerApiClient
    {
        public IMedRunnerTokenProvider TokenProvider { get; } = tokenProvider;
        public IAuthEndpoint Auth { get; } = authEndpoint;
        public IChatMessageEndpoint ChatMessage => throw new NotSupportedException();
        public IClientEndpoint Client => throw new NotSupportedException();
        public ICodeEndpoint Code => throw new NotSupportedException();
        public IEmergencyEndpoint Emergency => throw new NotSupportedException();
        public IOrgSettingsEndpoint OrgSettings => throw new NotSupportedException();
        public IStaffEndpoint Staff => throw new NotSupportedException();
        public IWebSocketEndpoint WebSocket => throw new NotSupportedException();
    }

    private sealed class SuccessfulAuthEndpoint(TokenGrant grant) : IAuthEndpoint
    {
        public Task<ApiResponse<TokenGrant>> RequestTokenAsync(string refreshToken)
            => Task.FromResult(new ApiResponse<TokenGrant>(grant));

        public Task<ApiResponse<string>> SignOutAsync(SignOutRequest? oldToken = null)
            => throw new NotSupportedException();

        public Task<ApiResponse<List<ApiToken>>> GetApiTokensAsync()
            => throw new NotSupportedException();

        public Task<ApiResponse<string>> CreateApiTokenAsync(CreateApiTokenRequest newToken)
            => throw new NotSupportedException();

        public Task<ApiResponse<string>> DeleteApiTokenAsync(string id)
            => throw new NotSupportedException();
    }

    private static TokenGrant CreateValidGrant()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(10);
        var token = new JsonWebTokenHandler().CreateToken(
            new SecurityTokenDescriptor
            {
                Issuer = "medrunner.space",
                Expires = expiresAt.UtcDateTime,
                Claims = new Dictionary<string, object>
                {
                    ["unique_name"] = "KronnY",
                },
            }
        );

        return new TokenGrant
        {
            AccessToken = token,
            RefreshToken = "candidate-api-token",
            AccessTokenExpiration = expiresAt,
        };
    }

    private sealed class StaticResponseHandler(Func<HttpRequestMessage, HttpResponseMessage> createResponse) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(createResponse(request));
    }

    private sealed class EphemeralMemoryCache : IMemoryCache
    {
        public bool TryGetValue(object key, out object? value)
        {
            value = null;
            return false;
        }

        public ICacheEntry CreateEntry(object key)
            => new EphemeralCacheEntry(key);

        public void Remove(object key)
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class EphemeralCacheEntry(object key) : ICacheEntry
    {
        public object Key { get; } = key;
        public object? Value { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public IList<IChangeToken> ExpirationTokens { get; } = [];
        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = [];
        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }

        public void Dispose()
        {
        }
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<LogLevel> LogLevels { get; } = [];
        public List<(LogLevel Level, string Message)> Entries { get; } = [];
        public IEnumerable<string> Messages => Entries.Select(entry => entry.Message);

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LogLevels.Add(logLevel);
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
