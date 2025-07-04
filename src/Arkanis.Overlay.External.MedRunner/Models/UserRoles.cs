namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Enum for user roles.
/// </summary>
public enum UserRoles : long
{
    Client = 1 << 0,
    Staff = 1 << 1,
    Developer = 1L << 51,
    Bot = 1L << 52,
}
