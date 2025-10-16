namespace Arkanis.Overlay.Common.Services;

using System.Security.Claims;
using Errors;
using Extensions;
using Models;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Results;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Result = FluentResults.Result;

public abstract class OidcAuthenticator(IServiceProvider serviceProvider) : ExternalAuthenticator<OidcAuthenticator.AuthenticationTask>
{
    public abstract OidcClient OidcClient { get; }

    public override Result ValidateCredentials(AccountCredentials? serviceCredentials)
        => serviceCredentials switch
        {
            AccountOidcCredentials => Result.Ok(),
            null => Result.Ok(), // no credentials are valid (means no authentication)
            _ => Result.Fail("Provided credentials are not valid OIDC credentials."),
        };

    public override AuthenticationTask AuthenticateAsync(AccountCredentials credentials, CancellationToken cancellationToken)
        => ActivatorUtilities.CreateInstance<AuthenticationTask>(serviceProvider, this, credentials, cancellationToken);

    public class AuthenticationTask(
        OidcAuthenticator authenticator,
        AccountCredentials credentials,
        ILogger<AuthenticationTask> logger,
        CancellationToken cancellationToken
    ) : AuthTaskBase(credentials, cancellationToken)
    {
        public override ExternalAuthenticatorInfo ProviderInfo
            => authenticator.AuthenticatorInfo;

        private OidcClient OidcClient
            => authenticator.OidcClient;

        protected override async Task<Result<ClaimsIdentity>> RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (Credentials is not AccountOidcCredentials oidcCredentials)
                {
                    return Result.Fail("Provided credentials are not valid OIDC credentials.");
                }

                var result = await OidcClient.GetUserInfoAsync(oidcCredentials.AccessToken, cancellationToken);
                AuthenticateCore(result);

                return IsAuthenticated switch
                {
                    true => Result.Ok(Identity),
                    false => Result.Fail(new ExternalAccountError(result.ErrorDescription ?? "Could not authenticate with the provided secret key.")),
                };
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process OIDC authentication to {Authority}", OidcClient.Options.Authority);
                return exception.ToResult();
            }
        }

        private void AuthenticateCore(UserInfoResult userResult)
            => Identity = userResult switch
            {
                { IsError: false } => CreateClaimsIdentity(userResult.Claims),
                _ => new ClaimsIdentity(),
            };

        protected virtual ClaimsIdentity CreateClaimsIdentity(IEnumerable<Claim> claims)
            => new(claims, ProviderInfo.ServiceId);
    }
}
