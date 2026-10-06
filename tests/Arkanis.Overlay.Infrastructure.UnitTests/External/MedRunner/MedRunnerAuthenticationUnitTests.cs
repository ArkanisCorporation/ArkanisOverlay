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
using Arkanis.Overlay.External.MedRunner.API.Endpoints.Emergency.Request;
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
        response.ErrorMessage.ShouldBe("MedRunner API request failed with status 401 (Unauthorized).");
    }

    private sealed class TestApiEndpoint(
        IMedRunnerClientConfig config,
        IMedRunnerTokenProvider tokenProvider,
        IMemoryCache cache,
        ILogger<TestApiEndpoint> logger,
        HttpClient? httpClient = null
    ) : ApiEndpoint(config, tokenProvider, cache, logger, httpClient)
    {
        protected override string Endpoint
            => "test";

        public Task<HttpRequestMessage> CreateAuthenticatedRequestAsync()
            => CreateRequestMessageAsync(HttpMethod.Get, "https://api.medrunner.space/test");

        public Task<HttpRequestMessage> CreateEmergencyRequestAsync(CreateEmergencyRequest body)
            => CreateRequestMessageAsync(HttpMethod.Post, "https://api.medrunner.space/emergency/", body);

        public Task<ApiResponse<string>> GetUnauthenticatedAsync()
            => GetRequestAsync<string>("", requestOptions: RequestOptions.Unauthenticated);

        public Task<ApiResponse<string>> GetAuthenticatedAsync()
            => GetRequestAsync<string>("");
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

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => LogLevels.Add(logLevel);
    }
}
