namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using System.Net;
using Abstractions.Endpoints;
using API.Endpoints.ChatMessage.Request;
using API.Endpoints.Emergency.Request;
using API.Endpoints.Emergency.Response;
using Models;

public class MockEmergencyEndpoint(
    MockClientEndpoint clientEndpoint,
    MockChatMessageEndpoint chatMessageEndpoint,
    MockWebSocketEventProvider eventProvider
) : MockApiEndpoint, IEmergencyEndpoint
{
    public Dictionary<string, Emergency> Emergencies { get; } = [];

    public Dictionary<string, List<ResponderDetails>> Responders { get; } = [];

    public Task<ApiResponse<Emergency>> GetEmergencyAsync(string emergencyId)
        => Emergencies.TryGetValue(emergencyId, out var emergency)
            ? OkResponseAsync(emergency)
            : EmergencyNotFoundResponseAsync<Emergency>(emergencyId);

    public Task<ApiResponse<List<Emergency>>> GetEmergenciesAsync(List<string> emergencyIds)
    {
        var emergencies = emergencyIds
            .Where(id => Emergencies.ContainsKey(id))
            .Select(id => Emergencies[id])
            .ToList();

        return OkResponseAsync(emergencies);
    }

    public Task<ApiResponse<Emergency>> CreateEmergencyAsync(CreateEmergencyRequest request)
    {
        var respondingTeam = new RespondingTeam
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Flying Dutchman",
        };
        var emergency = new Emergency
        {
            Id = Guid.NewGuid().ToString(),
            Status = MissionStatus.Pending,
            CreationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ClientId = clientEndpoint.Person.Id,
            System = request.Location.System,
            Subsystem = request.Location.Subsystem,
            TertiaryLocation = request.Location.TertiaryLocation,
            ThreatLevel = request.ThreatLevel,
            ClientRsiHandle = clientEndpoint.Person.RsiHandle ?? request.RsiHandle ?? "__UNKNOWN__",
            SubscriptionTier = "Budget",
            RespondingTeam = respondingTeam,
            RespondingTeams = [respondingTeam],
            Test = true,
            MissionName = $"Test Mission {Emergencies.Count + 1}",
            Origin = Origin.System,
        };

        Emergencies[emergency.Id] = emergency;
        eventProvider.SendNewEmergency(emergency);
        _ = PerformBackgroundUpdatesAsync(emergency);

        return OkResponseAsync(emergency);
    }

    public Task<ApiResponse<string>> CancelEmergencyWithReasonAsync(string emergencyId, CancellationReason reason)
    {
        if (!Emergencies.TryGetValue(emergencyId, out var emergency))
        {
            return EmergencyNotFoundResponseAsync<string>(emergencyId);
        }

        emergency.Status = MissionStatus.Cancelled;
        emergency.CancellationReason = reason;
        emergency.CompletionTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        eventProvider.SendEmergencyUpdate(emergency);

        return OkResponseAsync(emergencyId);
    }

    public Task<ApiResponse<string>> RateServicesAsync(string emergencyId, ResponseRating rating, string? remarks = null)
    {
        if (!Emergencies.TryGetValue(emergencyId, out var emergency))
        {
            return EmergencyNotFoundResponseAsync<string>(emergencyId);
        }

        emergency.Rating = rating;
        emergency.RatingRemarks = remarks;
        eventProvider.SendEmergencyUpdate(emergency);

        return OkResponseAsync(emergencyId);
    }

    public Task<ApiResponse<TeamDetailsResponse>> TeamDetailsAsync(string emergencyId)
    {
        if (!Responders.TryGetValue(emergencyId, out var responders))
        {
            return EmergencyNotFoundResponseAsync<TeamDetailsResponse>(emergencyId);
        }

        var response = new TeamDetailsResponse
        {
            Stats = responders,
            AggregatedSuccessRate = 0.0,
        };

        return OkResponseAsync(response);
    }

    private List<ResponderDetails> GetOrCreateRespondersFor(string emergencyId)
    {
        if (!Responders.TryGetValue(emergencyId, out var messages))
        {
            messages = Responders[emergencyId] = [];
        }

        return messages;
    }

    private async Task PerformBackgroundUpdatesAsync(Emergency emergency)
    {
        await UpdateAsync(() =>
            {
                emergency.Status = MissionStatus.Accepted;
                emergency.AcceptedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        );

        for (var i = 0; i < 3; i++)
        {
            await TaskUpdateAsync(() => chatMessageEndpoint.SendInternalMessageAsync(
                    new ChatMessageRequest
                    {
                        EmergencyId = emergency.Id,
                        Contents = "Test message",
                    }
                )
            );
        }

        await UpdateAsync(() =>
            {
                emergency.Status = MissionStatus.Completed;
                emergency.CompletionTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        );

        return;

        async Task UpdateAsync(Action update)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            update();
            eventProvider.SendEmergencyUpdate(emergency);
        }

        async Task TaskUpdateAsync(Func<Task> update)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            await update();
            eventProvider.SendEmergencyUpdate(emergency);
        }
    }

    private static Task<ApiResponse<T>> EmergencyNotFoundResponseAsync<T>(string id) where T : class
        => ErrorResponseAsync<T>($"Emergency with ID {id} not found.", HttpStatusCode.NotFound);
}
