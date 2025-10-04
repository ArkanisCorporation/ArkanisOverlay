namespace Arkanis.Overlay.LocalLink.Models.Commands;

using Common.Options;

public class SetExternalServiceCredentialsCommand : LocalLinkCommandBase
{
    public required UserPreferences.Credentials Credentials { get; init; }
}
