namespace Arkanis.Overlay.Common.Services;

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Results;
using Errors;
using Extensions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Models;
using Result = FluentResults.Result;

public abstract class OidcAuthenticator(IServiceProvider serviceProvider) : ExternalAuthenticator<OidcAuthenticator.AuthenticationTask>
{
    public abstract OidcClient OidcClient { get; }

    public virtual Options CurrentOptions { get; } = new();

    [field: MaybeNull]
    public virtual TokenValidationParameters ValidationParameters
        => field
           ?? new TokenValidationParameters
           {
               ValidIssuer = OidcClient.Options.Authority,
               ValidAudience = OidcClient.Options.ClientId,
               SignatureValidator = (token, _) =>
               {
                   var handler = new JsonWebTokenHandler();
                   return handler.ReadJsonWebToken(token);
               },
           };

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
        private readonly JsonWebTokenHandler _tokenHandler = new();

        public override ExternalAuthenticatorInfo ProviderInfo
            => authenticator.AuthenticatorInfo;

        private OidcClient OidcClient
            => authenticator.OidcClient;

        private TokenValidationParameters ValidationParameters
            => authenticator.ValidationParameters;

        protected override async Task<Result<ClaimsIdentity>> RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.CompletedTask;
                if (Credentials is not AccountOidcCredentials oidcCredentials)
                {
                    return Result.Fail("Provided credentials are not valid OIDC credentials.");
                }

                return await VerifyAsync(oidcCredentials, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process OIDC authentication to {Authority}", OidcClient.Options.Authority);
                return exception.ToResult();
            }
        }

        private async Task<Result<ClaimsIdentity>> VerifyAsync(AccountOidcCredentials oidcCredentials, CancellationToken cancellationToken)
        {
            var validationResult = await _tokenHandler.ValidateTokenAsync(oidcCredentials.IdToken ?? string.Empty, ValidationParameters);
            logger.LogDebug("OIDC token validation result: {@ValidationResult}", validationResult);

            if (validationResult.IsValid)
            {
                AuthenticateCore(validationResult.ClaimsIdentity);
                return Result.Ok(Identity);
            }

            logger.LogDebug("Fetching user info from OIDC provider");
            var result = await OidcClient.GetUserInfoAsync(oidcCredentials.AccessToken, cancellationToken);
            AuthenticateCore(result);

            return IsAuthenticated switch
            {
                true => Result.Ok(Identity),
                false => Result.Fail(
                    new ExternalAccountError(
                        result.ErrorDescription ?? "Could not authenticate with the provided secret key.",
                        validationResult.Exception.ToError()
                    )
                ),
            };
        }

        private void AuthenticateCore(ClaimsIdentity claimsIdentity)
            => Identity = CreateClaimsIdentity(claimsIdentity.Claims);

        private void AuthenticateCore(UserInfoResult userResult)
            => Identity = userResult switch
            {
                { IsError: false } => CreateClaimsIdentity(userResult.Claims),
                _ => new ClaimsIdentity(),
            };

        protected virtual ClaimsIdentity CreateClaimsIdentity(IEnumerable<Claim> claims)
        {
            var claimList = claims.ToList();

            foreach (var claimType in authenticator.CurrentOptions.DisplayNameClaimTypes)
            {
                if (claimList.FirstOrDefault(x => x.Type == claimType) is not { } claim)
                {
                    continue;
                }

                claimList.Add(new Claim(AccountClaimTypes.DisplayName, claim.Value));
                break;
            }

            foreach (var claimType in authenticator.CurrentOptions.AvatarUrlClaimTypes)
            {
                if (claimList.FirstOrDefault(x => x.Type == claimType) is not { } claim)
                {
                    continue;
                }

                claimList.Add(new Claim(AccountClaimTypes.AvatarUrl, claim.Value));
                break;
            }

            return new ClaimsIdentity(claimList, ProviderInfo.ServiceId, AccountClaimTypes.DisplayName, null);
        }
    }

    public class Options
    {
        public string[] AvatarUrlClaimTypes { get; set; } = ["picture"];

        public string[] DisplayNameClaimTypes { get; set; } =
        [
            JwtRegisteredClaimNames.Nickname, JwtRegisteredClaimNames.PreferredUsername, JwtRegisteredClaimNames.Name,
        ];
    }
}
