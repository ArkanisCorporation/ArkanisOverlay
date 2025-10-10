namespace Arkanis.Overlay.LocalLink.Models.Commands;

using Common.Models;

public class SetExternalServiceCredentialsCommand : LocalLinkCommandBase
{
    public required AccountCredentials Credentials { get; init; }
}
