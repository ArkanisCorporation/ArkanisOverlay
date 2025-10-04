namespace Arkanis.Overlay.LocalLink.Models;

using System.Text.Json.Serialization;
using Commands;

[JsonPolymorphic]
[JsonDerivedType(typeof(TestCommand), "test")]
[JsonDerivedType(typeof(SetExternalServiceCredentialsCommand), "v1/set/external-service-credentials")]
public abstract class LocalLinkCommandBase
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
