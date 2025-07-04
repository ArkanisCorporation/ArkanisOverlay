namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents an emergency mission in the Medrunner system.
/// </summary>
public class Emergency
{
    /// <summary>System where the emergency occurred.</summary>
    public required string System { get; set; }

    /// <summary>Subsystem where the emergency occurred.</summary>
    public required string Subsystem { get; set; }

    /// <summary>Tertiary location, if any.</summary>
    public string? TertiaryLocation { get; set; }

    /// <summary>The threat level of the emergency.</summary>
    public ThreatLevel ThreatLevel { get; set; }

    /// <summary>The RSI handle of the client.</summary>
    public required string ClientRsiHandle { get; set; }

    /// <summary>The Discord ID of the client.</summary>
    public string? ClientDiscordId { get; set; }

    /// <summary>The client ID.</summary>
    public string? ClientId { get; set; }

    /// <summary>The subscription tier of the client.</summary>
    public required string SubscriptionTier { get; set; }

    /// <summary>The status of the emergency mission.</summary>
    public MissionStatus Status { get; set; }

    /// <summary>Alert message cache.</summary>
    public MessageCache? AlertMessage { get; set; }

    /// <summary>Client message cache.</summary>
    public MessageCache? ClientMessage { get; set; }

    /// <summary>Coordination thread message cache.</summary>
    public MessageCache? CoordinationThread { get; set; }

    /// <summary>After action report message cache.</summary>
    public MessageCache? AfterActionReportMessage { get; set; }

    /// <summary>The primary responding team.</summary>
    public required Team RespondingTeam { get; set; }

    /// <summary>All responding teams.</summary>
    public required List<RespondingTeam> RespondingTeams { get; set; }

    /// <summary>Timestamp when the emergency was created.</summary>
    public long CreationTimestamp { get; set; }

    /// <summary>Timestamp when the emergency was accepted.</summary>
    public long? AcceptedTimestamp { get; set; }

    /// <summary>Timestamp when the emergency was completed.</summary>
    public long? CompletionTimestamp { get; set; }

    /// <summary>The rating for the emergency response.</summary>
    public ResponseRating Rating { get; set; }

    /// <summary>Remarks for the rating.</summary>
    public string? RatingRemarks { get; set; }

    /// <summary>Whether this is a test emergency.</summary>
    public bool Test { get; set; }

    /// <summary>The cancellation reason, if any.</summary>
    public CancellationReason CancellationReason { get; set; }

    /// <summary>The refusal reason, if any.</summary>
    public string? RefusalReason { get; set; }

    /// <summary>The origin of the emergency.</summary>
    public Origin Origin { get; set; }

    /// <summary>Client data for the emergency.</summary>
    public ClientData? ClientData { get; set; }

    /// <summary>Whether the emergency is complete.</summary>
    public bool IsComplete { get; set; }

    /// <summary>The mission name, if any.</summary>
    public string? MissionName { get; set; }

    /// <summary>The after action report, if any.</summary>
    public AfterActionReport? AfterActionReport { get; set; }

    /// <summary>The submission source.</summary>
    public SubmissionSource SubmissionSource { get; set; }
}
