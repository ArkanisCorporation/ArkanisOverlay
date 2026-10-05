namespace Arkanis.Overlay.Components.Shared;

using Overlay.External.MedRunner.API;
using Overlay.External.MedRunner.Models;

public enum EmergencyDetailsStep
{
    Situation = 1,
    Location = 2,
    Players = 3,
    Remarks = 4,
}

public sealed record EmergencyDetailsProgress(IReadOnlyList<EmergencyDetailsStep> CompletedSteps, bool Skipped)
{
    public bool IsComplete
        => CompletedSteps.Count == Enum.GetValues<EmergencyDetailsStep>().Length;

    public EmergencyDetailsStep? NextIncompleteStep
        => Enum.GetValues<EmergencyDetailsStep>()
            .FirstOrDefault(step => !CompletedSteps.Contains(step)) is var step and not 0
            ? step
            : null;
}

/// <summary>
///     Creates and restores the durable, client-authored emergency details messages.
/// </summary>
public static class MedRunnerEmergencyDetailsProgress
{
    private const string DetailsHeadingPrefix = "## Emergency details: ";
    private const string SkippedTitle = $"{DetailsHeadingPrefix}Skipped";
    private const string LegacyBundledSituationLocationMarker = "_The client type of location is:_";

    public static string CreateStepMessage(EmergencyDetailsStep step, string contents)
        => $"{GetStepTitle(step)}\n\n{contents}";

    public static string CreateSkippedMessage()
        => $"{SkippedTitle}\n\n_The client chose not to provide additional emergency details._";

    public static bool CanSendSkippedMessage(EmergencyDetailsProgress progress)
        => !progress.Skipped;

    public static EmergencyDetailsProgress GetProgress(IEnumerable<ChatMessage> messages, string clientId)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentException.ThrowIfNullOrEmpty(clientId);

        var clientMessages = messages
            .Where(message => string.Equals(message.SenderId, clientId, StringComparison.Ordinal))
            .ToArray();
        var hasLegacyBundledSituation = clientMessages.Any(message =>
            message.Content.StartsWith(GetStepTitle(EmergencyDetailsStep.Situation), StringComparison.Ordinal)
            && message.Content.Contains(LegacyBundledSituationLocationMarker, StringComparison.Ordinal)
        );
        var completedSteps = Enum.GetValues<EmergencyDetailsStep>()
            .Where(step => clientMessages.Any(message => message.Content.StartsWith(GetStepTitle(step), StringComparison.Ordinal))
                           || (step is EmergencyDetailsStep.Location && hasLegacyBundledSituation))
            .ToArray();
        var skipped = completedSteps.Length == 0
                      && clientMessages.Any(message => message.Content.StartsWith(SkippedTitle, StringComparison.Ordinal));

        return new EmergencyDetailsProgress(completedSteps, skipped);
    }

    public static async Task<EmergencyDetailsProgress> LoadProgressAsync(
        Func<string?, Task<ApiPaginatedResponse<ChatMessage>>> loadPageAsync,
        string clientId
    )
    {
        ArgumentNullException.ThrowIfNull(loadPageAsync);

        var messages = new List<ChatMessage>();
        var seenPaginationTokens = new HashSet<string>(StringComparer.Ordinal);
        string? paginationToken = null;

        while (true)
        {
            var page = await loadPageAsync(paginationToken);
            messages.AddRange(page.Data);

            var nextPaginationToken = page.PaginationToken;
            if (string.IsNullOrEmpty(nextPaginationToken) || !seenPaginationTokens.Add(nextPaginationToken))
            {
                break;
            }

            paginationToken = nextPaginationToken;
        }

        return GetProgress(messages, clientId);
    }

    public static string GetStepTitle(EmergencyDetailsStep step)
        => step switch
        {
            EmergencyDetailsStep.Situation => $"{DetailsHeadingPrefix}Situation",
            EmergencyDetailsStep.Location => $"{DetailsHeadingPrefix}Location",
            EmergencyDetailsStep.Players => $"{DetailsHeadingPrefix}Players",
            EmergencyDetailsStep.Remarks => $"{DetailsHeadingPrefix}Remarks",
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, null),
        };
}
