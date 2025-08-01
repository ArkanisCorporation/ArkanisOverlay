namespace Arkanis.Overlay.Infrastructure.Services.External;

using System.Diagnostics.CodeAnalysis;
using System.Net;
using Domain;
using Domain.Abstractions.Services;
using Domain.Options;
using Exceptions;
using Overlay.External.UEX;
using Overlay.External.UEX.Abstractions;

public class UexAccountContext(IUexUserApi userApi, IUserPreferencesManager userPreferences, UexApiOptions options) : SelfInitializableServiceBase
{
    public UexUserDTO? CurrentUser { get; set; }

    [MemberNotNullWhen(true, nameof(CurrentUser))]
    public bool IsLinked
        => CurrentUser is not null;

    public Exception? LinkError { get; private set; }

    private UserPreferences.Credentials Credentials
        => userPreferences.CurrentPreferences.GetOrCreateCredentialsFor(ExternalService.UnitedExpress);

    public async Task ConfigureAsync(UserPreferences.Credentials credentials, CancellationToken cancellationToken)
    {
        if (credentials is not { UserIdentifier.Length: > 0, SecretToken.Length: > 0 })
        {
            throw new ExternalLinkUnauthorizedException("Provided secret key is not valid.", null);
        }

        options.UserToken = credentials.SecretToken;

        try
        {
            var userResponse = await userApi.GetUserAsync(credentials.UserIdentifier, cancellationToken);
            if (IsLinkedUserValid(userResponse))
            {
                var persistentCredentials = Credentials;
                persistentCredentials.UserIdentifier = credentials.UserIdentifier;
                persistentCredentials.SecretToken = credentials.SecretToken;

                CurrentUser = userResponse.Result.Data;
                await userPreferences.SaveAndApplyUserPreferencesAsync(userPreferences.CurrentPreferences);
            }
            else
            {
                throw new ExternalLinkUnauthorizedException("Provided key is not valid or does not belong to the specified account.", null);
            }
        }
        catch (UexApiException exception)
        {
            var newException = exception.StatusCode switch
            {
                (int)HttpStatusCode.NotFound => new ExternalLinkAccountNotFoundException("Account with the provided username does not exist.", exception),
                (int)HttpStatusCode.Unauthorized => new ExternalLinkUnauthorizedException("Provided secret key is not valid.", exception),
                _ => new ExternalLinkException("Could not verify account with the provided username.", exception),
            };

            throw newException;
        }
        catch (Exception exception) when (exception is not ExternalLinkException)
        {
            throw new ExternalLinkException($"Internal error has occured: {exception.Message}", exception);
        }
        finally
        {
            if (!IsLinked)
            {
                options.UserToken = null;
            }
        }
    }

    public async Task UpdateAsync(CancellationToken cancellationToken)
    {
        if (Credentials is not { UserIdentifier.Length: > 0, SecretToken.Length: > 0 })
        {
            return;
        }

        try
        {
            var userResponse = await userApi.GetUserAsync(Credentials.UserIdentifier, cancellationToken);
            if (IsLinkedUserValid(userResponse))
            {
                CurrentUser = userResponse.Result.Data;
                LinkError = null;
            }
            else
            {
                LinkError = new ExternalLinkUnauthorizedException("Provided key is not valid or does not belong to the specified account.", null);
            }
        }
        catch (Exception exception)
        {
            LinkError = exception;
        }
    }

    private bool IsLinkedUserValid(UexApiResponse<GetUserOkResponse> userResponse)
        => userResponse.Result.Data is not { Discord_username.Length: > 0 } or { Email.Length: > 0 };

    protected override async Task InitializeAsyncCore(CancellationToken cancellationToken)
        => await UpdateAsync(cancellationToken);
}
