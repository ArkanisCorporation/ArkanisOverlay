namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents a person in the system.
/// </summary>
public class Person : ModelBase
{
    /// <summary>
    ///     Whether the person is delinquent.
    /// </summary>
    public bool Delinquent { get; set; }

    /// <summary>
    ///     Whether the person is in a grace period.
    /// </summary>
    public bool Grace { get; set; }

    /// <summary>
    ///     Whether the person is overdue for payment.
    /// </summary>
    public bool Overdue { get; set; }

    /// <summary>
    ///     The Discord ID of the person.
    /// </summary>
    public required string DiscordId { get; set; }

    /// <summary>
    ///     The RSI handle of the person.
    /// </summary>
    public string? RsiHandle { get; set; }

    /// <summary>
    ///     The roles assigned to the person.
    /// </summary>
    public UserRoles Roles { get; set; }

    /// <summary>
    ///     The type of person (client, staff, bot).
    /// </summary>
    public PersonType PersonType { get; set; }

    /// <summary>
    ///     Whether the person is active.
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    ///     The reason for deactivation.
    /// </summary>
    public AccountDeactivationReason DeactivationReason { get; set; }

    /// <summary>
    ///     The client stats for the person.
    /// </summary>
    public required ClientStats ClientStats { get; set; }

    /// <summary>
    ///     The client portal preferences blob.
    /// </summary>
    public required object ClientPortalPreferences { get; set; }

    /// <summary>
    ///     The client portal preferences blob as a string.
    /// </summary>
    public string? ClientPortalPreferencesBlob { get; set; }

    /// <summary>
    ///     Whether the person allows anonymous alerts.
    /// </summary>
    public bool AllowAnonymousAlert { get; set; }

    /// <summary>
    ///     The initial join date.
    /// </summary>
    public DateTimeOffset? InitialJoinDate { get; set; }

    /// <summary>
    ///     The credit the person has.
    /// </summary>
    public double AccountCredit { get; set; }
}
