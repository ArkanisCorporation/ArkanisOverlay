namespace Arkanis.Overlay.Common.Models;

public record UpdateChannel(string Name, string InternalId, string? VelopackChannelId)
{
    public static readonly UpdateChannel Default = new("Current", "default", null);

    public static readonly UpdateChannel Stable = new("Stable", "stable", "stable");

    public static readonly UpdateChannel ReleaseCandidate = new("Release Candidate", "rc", "rc")
    {
        IsUnstable = true,
        Description = "Contains fairly stable changes close to a stable release. May contain bugs.",
    };

    public static readonly UpdateChannel Preview = new("Developer Preview", "preview", "preview")
    {
        IsUnstable = true,
        Description = "Contains latest development changes. May contain major bugs and breaking changes.",
    };

    private static readonly UpdateChannel Undefined = new("Unspecified", "unspecified", null)
    {
        IsUnstable = true,
        Description = "Local unreleased development version.",
    };

    public static readonly HashSet<UpdateChannel> Available =
    [
        Default,
        Stable,
        ReleaseCandidate,
        Preview,
    ];

    public static readonly IEnumerable<UpdateChannel> All =
    [
        Default,
        Stable,
        ReleaseCandidate,
        Preview,
    ];

    public bool IsUnstable { get; init; }
    public string? Description { get; init; }

    public static UpdateChannel ById(string? id)
        => All.SingleOrDefault(x => x.InternalId == id, Default);

    public static UpdateChannel ByVelopackChannelId(string? channelId)
        => All.SingleOrDefault(x => x.VelopackChannelId == channelId, Undefined);
}
