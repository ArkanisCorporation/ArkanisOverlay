namespace Arkanis.Overlay.External.MedRunner;

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using API;
using API.Abstractions;
using Common;
using Common.Models;
using Common.Services;
using FluentResults;

public sealed class MedRunnerAuthenticator(IMedRunnerTokenProvider tokenProvider) : ExternalAuthenticator<MedRunnerAuthenticator.AuthenticationTask>
{
    public static ExternalAuthenticatorInfo ProviderInfo { get; } = new()
    {
        Identifier = ExternalService.MedRunner,
        DisplayName = "MedRunner",
        Description = "MedRunner provides emergency-response services for Star Citizen pilots.",
    };

    public override ExternalAuthenticatorInfo AuthenticatorInfo
        => ProviderInfo;

    public override Result ValidateCredentials(AccountCredentials? serviceCredentials)
        => serviceCredentials switch
        {
            AccountApiTokenCredentials { SecretToken.Length: > 0 } => Result.Ok(),
            AccountApiTokenCredentials => Result.Fail("A MedRunner API token is required."),
            null => Result.Ok(),
            _ => Result.Fail("Provided credentials are not valid MedRunner API token credentials."),
        };

    public override AuthenticationTask AuthenticateAsync(AccountCredentials credentials, CancellationToken cancellationToken)
        => new(tokenProvider, credentials, cancellationToken);

    public sealed class AuthenticationTask(
        IMedRunnerTokenProvider tokenProvider,
        AccountCredentials credentials,
        CancellationToken cancellationToken
    ) : AuthTaskBase(credentials, cancellationToken)
    {
        public override ExternalAuthenticatorInfo ProviderInfo
            => MedRunnerAuthenticator.ProviderInfo;

        public MedRunnerTokenAuthentication? Authentication { get; private set; }

        [MemberNotNullWhen(true, nameof(Authentication))]
        public override bool IsAuthenticated
            => Authentication is not null && base.IsAuthenticated;

        protected override async Task<Result<ClaimsIdentity>> RunAsync(CancellationToken cancellationToken)
        {
            if (Credentials is not AccountApiTokenCredentials tokenCredentials)
            {
                return Result.Fail<ClaimsIdentity>("Provided credentials are not valid MedRunner API token credentials.");
            }

            var authenticationResult = await tokenProvider.AuthenticateApiTokenAsync(tokenCredentials.SecretToken, cancellationToken);
            if (authenticationResult.IsFailed)
            {
                return Result.Fail<ClaimsIdentity>(authenticationResult.Errors);
            }

            Authentication = authenticationResult.Value;
            Identity = Authentication.Identity;
            return Result.Ok(Identity);
        }
    }
}
