namespace Arkanis.Overlay.External.MedRunner.Models;

using System.Text.Json;

/// <summary>
///     The response roster attached to an emergency, including an empty roster before responders are assigned.
/// </summary>
public sealed class EmergencyResponseTeam
{
    public int MaxMembers { get; set; }

    // Preserve member payloads until their API shape is known; they are not team identities.
    public List<JsonElement> Staff { get; set; } = [];
    public List<JsonElement> Dispatchers { get; set; } = [];
    public List<JsonElement> AllMembers { get; set; } = [];
}
