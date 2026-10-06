namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents a deployment.
/// </summary>
public class Deployment
{
    /// <summary>
    ///     The type of client for the deployment.
    /// </summary>
    public ClientType ClientType { get; set; }

    /// <summary>
    ///     The version of the deployment.
    /// </summary>
    public required string Version { get; set; }
}

/// <summary>
///     The type of client for the deployment.
/// </summary>
public enum ClientType
{
    ClientPortal = 1,
    StaffPortal = 2,
}
