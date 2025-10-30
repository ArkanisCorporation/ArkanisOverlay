namespace Arkanis.Overlay.Infrastructure.Services;

using Common.Models;
using Common.Services;

public class ExternalAuthenticatorProvider(IEnumerable<ExternalAuthenticator> externalAuthenticators)
{
    public ExternalAuthenticator? GetForCredentials(AccountCredentials credentials)
        => externalAuthenticators.FirstOrDefault(x => x.AuthenticatorInfo.ServiceId == credentials.ServiceId);

    public ExternalAuthenticator.AuthTaskBase AuthenticateWith(AccountCredentials credentials, CancellationToken cancellationToken)
        => GetForCredentials(credentials) switch
        {
            { } authenticator => authenticator.AuthenticateAsync(credentials, cancellationToken),
            _ => new ExternalAuthenticator.AuthenticatorMissingTask(credentials),
        };
}
