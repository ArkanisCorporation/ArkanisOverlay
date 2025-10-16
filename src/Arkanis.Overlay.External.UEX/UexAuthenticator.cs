namespace Arkanis.Overlay.External.UEX;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using Abstractions;
using Common.Errors;
using Common.Extensions;
using Common.Models;
using Common.Services;
using Extensions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class UexAuthenticator(IServiceProvider serviceProvider) : ExternalAuthenticator<UexAuthenticator.AuthenticationTask>
{
    public override ExternalAuthenticatorInfo AuthenticatorInfo
        => UexConstants.ProviderInfo;

    public override Result ValidateCredentials(AccountCredentials? serviceCredentials)
        => serviceCredentials switch
        {
            AccountApiTokenCredentials => Result.Ok(),
            null => Result.Ok(), // no credentials are valid (means no authentication)
            _ => Result.Fail("Provided credentials are not valid UEX API token credentials."),
        };

    public override AuthenticationTask AuthenticateAsync(AccountCredentials credentials, CancellationToken cancellationToken)
        => ActivatorUtilities.CreateInstance<AuthenticationTask>(serviceProvider, credentials, cancellationToken);

    public class AuthenticationTask(
        IUexUserApi userApi,
        ILogger<AuthenticationTask> logger,
        AccountCredentials credentials,
        CancellationToken cancellationToken
    )
        : AuthTaskBase(credentials, cancellationToken)
    {
        public override ExternalAuthenticatorInfo ProviderInfo
            => UexConstants.ProviderInfo;

        public UexUserDTO? User { get; private set; }

        [MemberNotNullWhen(true, nameof(User))]
        public override bool IsAuthenticated
            => User is not null && base.IsAuthenticated;

        protected override async Task<Result<ClaimsIdentity>> RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (Credentials is not AccountApiTokenCredentials tokenCredentials)
                {
                    return Result.Fail("Provided credentials are not valid UEX API token credentials.");
                }

                var scopedApi = userApi.WithOverrideOptions(opts => opts.UserToken = tokenCredentials.SecretToken);
                var response = await scopedApi.GetUserAsync(null, CancellationToken.None);
                AuthenticateCore(response);

                return IsAuthenticated switch
                {
                    true => Result.Ok(Identity),
                    false => Result.Fail(new ExternalAccountError("Could not authenticate with the provided secret key.")),
                };
            }
            catch (UexApiException exception)
            {
                var causedBy = exception.ToError();
                var error = exception.StatusCode switch
                {
                    (int)HttpStatusCode.NotFound => new ExternalAccountNotFoundError("Account with the provided credentials could not be found.", causedBy),
                    (int)HttpStatusCode.Unauthorized => new ExternalAccountUnauthorizedError("Provided secret key is not valid.", causedBy),
                    _ => new ExternalAccountError("Could not verify account with the provided secret key.", causedBy),
                };

                logger.LogWarning(exception, "Could not authenticate UEX account");
                return Result.Fail(error);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process UEX authentication");
                return exception.ToResult();
            }
        }

        private void AuthenticateCore(UexApiResponse<GetUserOkResponse> userResponse)
        {
            User = userResponse.Result.Data;
            Identity = User switch
            {
                { } user => CreateClaimsIdentity(user),
                _ => new ClaimsIdentity(),
            };
        }

        private ClaimsIdentity CreateClaimsIdentity(UexUserDTO user)
        {
            var claims = new List<Claim>();
            if (user.Id is not null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (user.Username is not null)
            {
                claims.Add(new Claim(ClaimTypes.Name, user.Username));
            }

            if (user.Name is not null)
            {
                claims.Add(new Claim(AccountClaimTypes.DisplayName, user.Name));
            }

            if (user.Avatar is not null)
            {
                claims.Add(new Claim(AccountClaimTypes.AvatarUrl, user.Avatar));
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            if (!string.IsNullOrWhiteSpace(user.Website_url))
            {
                claims.Add(new Claim(ClaimTypes.Webpage, user.Website_url));
            }

            return new ClaimsIdentity(claims, ProviderInfo.ServiceId, AccountClaimTypes.DisplayName, null);
        }
    }
}
