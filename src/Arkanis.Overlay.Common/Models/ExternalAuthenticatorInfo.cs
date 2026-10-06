namespace Arkanis.Overlay.Common.Models;

public record ExternalAuthenticatorInfo
{
    public required string ServiceId { get; init; }

    public required string DisplayName { get; init; }

    public required string Description { get; init; }
}
