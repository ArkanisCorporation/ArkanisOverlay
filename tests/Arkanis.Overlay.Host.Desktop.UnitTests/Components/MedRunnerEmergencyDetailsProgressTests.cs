namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using Overlay.Components.Shared;
using Overlay.External.MedRunner.API;
using Overlay.External.MedRunner.Models;
using Shouldly;

public sealed class MedRunnerEmergencyDetailsProgressTests
{
    [Theory]
    [InlineData(EmergencyDetailsStep.Situation, "Situation")]
    [InlineData(EmergencyDetailsStep.Location, "Location")]
    [InlineData(EmergencyDetailsStep.Players, "Players")]
    [InlineData(EmergencyDetailsStep.Remarks, "Remarks")]
    public void Step_message_has_a_stable_section_heading(EmergencyDetailsStep step, string section)
    {
        MedRunnerEmergencyDetailsProgress.CreateStepMessage(step, "body")
            .ShouldBe($"## Emergency details: {section}\n\nbody");
    }

    [Fact]
    public void Progress_uses_only_client_messages_and_preserves_legacy_location_progress()
    {
        var progress = MedRunnerEmergencyDetailsProgress.GetProgress(
            [
                Message("client", "## Emergency details: Situation\n\n_The client type of location is:_  **Bunker**"),
                Message("dispatcher", "## Emergency details: Players"),
            ],
            "client"
        );

        progress.CompletedSteps.ShouldBe(
        [
            EmergencyDetailsStep.Situation,
            EmergencyDetailsStep.Location,
        ]);
        progress.Skipped.ShouldBeFalse();
        progress.NextIncompleteStep.ShouldBe(EmergencyDetailsStep.Players);
    }

    [Fact]
    public void Skipped_state_requires_no_completed_detail_steps()
    {
        var progress = MedRunnerEmergencyDetailsProgress.GetProgress(
            [
                Message("client", MedRunnerEmergencyDetailsProgress.CreateSkippedMessage()),
                Message("client", MedRunnerEmergencyDetailsProgress.CreateStepMessage(EmergencyDetailsStep.Remarks, "_Remarks:_  <em>None provided</em>")),
            ],
            "client"
        );

        progress.CompletedSteps.ShouldBe([EmergencyDetailsStep.Remarks]);
        progress.Skipped.ShouldBeFalse();
    }

    [Fact]
    public async Task LoadProgress_reads_all_unique_history_pages()
    {
        var requestedTokens = new List<string?>();

        var progress = await MedRunnerEmergencyDetailsProgress.LoadProgressAsync(
            token =>
            {
                requestedTokens.Add(token);
                return Task.FromResult(token switch
                {
                    null => Page("first", "## Emergency details: Situation", "page-2"),
                    "page-2" => Page("second", "## Emergency details: Location", "page-2"),
                    _ => throw new InvalidOperationException($"Unexpected pagination token: {token}"),
                });
            },
            "client"
        );

        requestedTokens.ShouldBe([null, "page-2"]);
        progress.CompletedSteps.ShouldBe(
        [
            EmergencyDetailsStep.Situation,
            EmergencyDetailsStep.Location,
        ]);
    }

    private static ApiPaginatedResponse<ChatMessage> Page(string id, string content, string? paginationToken)
        => new()
        {
            Data = [Message("client", content, id)],
            PaginationToken = paginationToken,
        };

    private static ChatMessage Message(string senderId, string content, string? id = null)
        => new()
        {
            Id = id ?? Guid.NewGuid().ToString("N"),
            EmergencyId = "emergency",
            SenderId = senderId,
            Content = content,
        };
}
