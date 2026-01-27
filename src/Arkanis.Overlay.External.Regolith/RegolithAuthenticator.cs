namespace Arkanis.Overlay.External.Regolith;

using System.Security.Claims;
using Abstractions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Overlay.Common.Errors;
using Overlay.Common.Extensions;
using Overlay.Common.Models;
using Overlay.Common.Services;

public class RegolithAuthenticator(IServiceProvider serviceProvider) : ExternalAuthenticator<RegolithAuthenticator.AuthenticationTask>
{
    public override ExternalAuthenticatorInfo AuthenticatorInfo
        => RegolithConstants.ProviderInfo;

    public override Result ValidateCredentials(AccountCredentials? serviceCredentials)
        => serviceCredentials switch
        {
            AccountApiTokenCredentials { SecretToken.Length: > 0 } => Result.Ok(),
            null => Result.Ok(), // no credentials are valid (means no authentication)
            _ => Result.Fail("Provided credentials are not valid Regolith API key credentials."),
        };

    public override AuthenticationTask AuthenticateAsync(AccountCredentials credentials, CancellationToken cancellationToken)
        => ActivatorUtilities.CreateInstance<AuthenticationTask>(serviceProvider, credentials, cancellationToken);

    public class AuthenticationTask(
        IRegolithApiClient apiClient,
        ILogger<AuthenticationTask> logger,
        AccountCredentials credentials,
        CancellationToken cancellationToken
    )
        : AuthTaskBase(credentials, cancellationToken)
    {
        public override ExternalAuthenticatorInfo ProviderInfo
            => RegolithConstants.ProviderInfo;

        public string? ApiKey { get; private set; }

        public override bool IsAuthenticated
            => ApiKey is not null && base.IsAuthenticated;

        protected override async Task<Result<ClaimsIdentity>> RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (Credentials is not AccountApiTokenCredentials tokenCredentials)
                {
                    return Result.Fail("Provided credentials are not valid Regolith API key credentials.");
                }

                // Set the API key on the client
                apiClient.SetApiKey(tokenCredentials.SecretToken);

                // Verify by fetching lookups (minimal API call)
                var lookups = await apiClient.GetLookupsAsync(cancellationToken);

                if (lookups is null)
                {
                    apiClient.SetApiKey(null);
                    return Result.Fail(new ExternalAccountError("Could not authenticate with the provided API key."));
                }

                ApiKey = tokenCredentials.SecretToken;
                Identity = CreateClaimsIdentity(tokenCredentials);

                return Result.Ok(Identity);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process Regolith authentication");
                apiClient.SetApiKey(null);
                return exception.ToResult();
            }
        }

        private ClaimsIdentity CreateClaimsIdentity(AccountApiTokenCredentials credentials)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, credentials.UserIdentifier ?? "Regolith User"),
                new(AccountClaimTypes.DisplayName, credentials.UserIdentifier ?? "Regolith User"),
            };

            return new ClaimsIdentity(claims, ProviderInfo.ServiceId, AccountClaimTypes.DisplayName, null);
        }
    }
}
