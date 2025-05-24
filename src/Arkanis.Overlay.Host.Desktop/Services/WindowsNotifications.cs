namespace Arkanis.Overlay.Host.Desktop.Services;

using System.Globalization;
using System.Runtime.InteropServices;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Domain.Options;
using Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using NuGet.Versioning;
using Velopack;

internal class WindowsNotifications : IDisposable
{
    private const string IdArgumentKey = "id";

    private const string UpdatesNotificationGroup = "Updates";
    private const string UpdateProgressNotificationTag = "UpdateProgress";

    private const string AppAttributionText = "Built with ❤️ by Arkanis Corporation\n(FatalMerlin, TheKronnY, and contributors)";

    private readonly HashSet<Notification> _notificationsShown = [];
    private readonly ToastNotifierCompat _notifier = ToastNotificationManagerCompat.CreateToastNotifier();

    public void Dispose()
    {
        foreach (var toastNotification in _notificationsShown.ToList())
        {
            try
            {
                toastNotification.Hide();
            }
            catch (COMException)
            {
            }
        }
    }

    private T ShowToast<T>(T notification) where T : Notification
    {
        _notificationsShown.Add(notification);
        _notifier.Show(notification.Toast);
        return notification;
    }

    public static void ShowWelcomeToast(UserPreferences userPreferences)
    {
        var launchShortcut = userPreferences.LaunchShortcut.Description;
        new ToastContentBuilder()
            .SetToastDuration(ToastDuration.Long)
            .SetToastScenario(ToastScenario.Default)
            .AddText("Welcome to the Arkanis Overlay!", AdaptiveTextStyle.Header)
            .AddText($"Press {launchShortcut} to toggle the Overlay while in game.", AdaptiveTextStyle.Body)
            .AddText("Right-click the tray icon to open the preferences or exit.", AdaptiveTextStyle.Body)
            .AddAttributionText(AppAttributionText)
            .Show();
    }

    public static void ShowUpdatedToast(SemanticVersion currentVersion)
        => new ToastContentBuilder()
            .AddText("Update complete", AdaptiveTextStyle.Header)
            .AddText($"The overlay was successfully updated to version {currentVersion}.", AdaptiveTextStyle.Body)
            .AddAttributionText(AppAttributionText)
            .Show(toast => toast.ExpirationTime = DateTimeOffset.Now.Add(TimeSpan.FromMinutes(1)));

    private async Task<PromptResult> PromptAsync(
        IEnumerable<PromptOption> options,
        TimeSpan timeout,
        Action<ToastContentBuilder> buildContent,
        CancellationToken cancellationToken
    )
    {
        var toastId = Guid.NewGuid().ToString();
        var builder = new ToastContentBuilder();
        buildContent(builder);

        foreach (var promptOption in options)
        {
            if (promptOption is PromptButtonOption buttonOption)
            {
                var button = new ToastButton(promptOption.Label, string.Empty)
                    .SetBackgroundActivation()
                    .AddArgument(PromptActivationResult.ActionArgumentKey, buttonOption.Action)
                    .AddArgument(IdArgumentKey, toastId);

                builder.AddButton(button);
            }
            else if (promptOption is PromptSnoozeOption)
            {
                builder.AddButton(new ToastButtonSnooze(promptOption.Label));
            }
        }

        var content = builder.GetToastContent();
        var notification = new PromptNotification(_notifier)
        {
            Toast = new ToastNotification(content.GetXml())
            {
                Group = UpdatesNotificationGroup,
                Tag = UpdateProgressNotificationTag,
                Data = UpdateProgressParams.Empty.CreateData(),
                ExpirationTime = DateTimeOffset.Now.Add(timeout),
            },
        };

        var activationTask = new TaskCompletionSource<PromptActivationResult>();
        ToastNotificationManagerCompat.OnActivated += OnToastActivated;

        try
        {
            ShowToast(notification);
            return await activationTask.Task.WaitAsync(timeout, cancellationToken);
        }
        catch (TimeoutException)
        {
            activationTask.SetCanceled(cancellationToken);
            return PromptTimedOutResult.Instance;
        }
        finally
        {
            ToastNotificationManagerCompat.OnActivated -= OnToastActivated;
            notification.Hide();
        }

        void OnToastActivated(ToastNotificationActivatedEventArgsCompat eventArgs)
        {
            var result = PromptActivationResult.CreateFrom(eventArgs.UserInput, eventArgs.Argument);
            if (result.Id == toastId)
            {
                activationTask.TrySetResult(result);
            }
        }
    }

    public async Task<bool> ShouldUpdateNowAsync(UpdateInfo updateInfo, CancellationToken cancellationToken)
    {
        const string updateNowAction = "updateNow";
        var result = await PromptAsync(
            [new PromptButtonOption("Update now", updateNowAction)],
            TimeSpan.FromMinutes(2),
            builder => builder
                .SetToastScenario(ToastScenario.Reminder)
                .AddText("Update Pending", AdaptiveTextStyle.Caption)
                .AddText("New overlay version is available!", AdaptiveTextStyle.Body)
                .AddText($"You can now update to version {updateInfo.TargetFullRelease.Version}.", AdaptiveTextStyle.BodySubtle),
            cancellationToken
        );

        return result is PromptActivationResult { Action: updateNowAction };
    }

    public UpdatableNotification<UpdateProgressParams> ShowUpdateProgressToast(UpdateInfo updateInfo)
    {
        var content = new ToastContentBuilder()
            .AddText("Downloading and installing application update...")
            .SetToastScenario(ToastScenario.Reminder)
            .AddVisualChild(
                new AdaptiveProgressBar
                {
                    Title = $"Updating to {updateInfo.TargetFullRelease.Version}",
                    Value = new BindableProgressBarValue(UpdateProgressParams.ProgressValueDataKey),
                    Status = new BindableString(UpdateProgressParams.ProgressStatusDataKey),
                }
            )
            .GetToastContent();

        var notification = new UpdatableNotification<UpdateProgressParams>(_notifier)
        {
            Toast = new ToastNotification(content.GetXml())
            {
                Group = UpdatesNotificationGroup,
                Tag = UpdateProgressNotificationTag,
                Data = UpdateProgressParams.Empty.CreateData(),
            },
        };
        return ShowToast(notification);
    }

    public abstract class PromptResult;

    public sealed class PromptTimedOutResult : PromptResult
    {
        private PromptTimedOutResult()
        {
        }

        public static PromptResult Instance { get; } = new PromptTimedOutResult();
    }

    public sealed class PromptActivationResult : PromptResult
    {
        public const string ActionArgumentKey = "action";

        public string? Id
            => Arguments.GetValueOrDefault(IdArgumentKey);

        public string? Action
            => Arguments.GetValueOrDefault(ActionArgumentKey);

        public required ValueSet UserInput { get; init; }
        public required Dictionary<string, string> Arguments { get; init; }

        public static PromptActivationResult CreateFrom(ValueSet userInput, string values)
            => new()
            {
                Arguments = ToastArgumentHelper.Parse(values),
                UserInput = userInput,
            };
    }

    public abstract record PromptOption(string Label);

    public record PromptButtonOption(string Label, string Action) : PromptOption(Label);

    public record PromptSnoozeOption(string Label) : PromptOption(Label);

    public abstract record Params
    {
        public abstract NotificationData CreateData();
    }

    public record UpdateProgressParams(int ProgressPercent) : Params
    {
        public const string ProgressValueDataKey = nameof(ProgressValueDataKey);
        public const string ProgressStatusDataKey = nameof(ProgressStatusDataKey);

        public static readonly UpdateProgressParams Empty = new(0);

        public override NotificationData CreateData()
            => new()
            {
                Values =
                {
                    [ProgressValueDataKey] = (ProgressPercent / 100.0).ToString(CultureInfo.InvariantCulture),
                    [ProgressStatusDataKey] = ProgressPercent switch
                    {
                        <= 0 => "Preparing...",
                        < 100 => "Downloading...",
                        >= 100 => "Installing...",
                    },
                },
                SequenceNumber = (uint)ProgressPercent,
            };
    }

    public class Notification(ToastNotifierCompat notifier)
    {
        private bool _hidden;

        protected ToastNotifierCompat Notifier { get; } = notifier;
        public required ToastNotification Toast { get; init; }

        public void Hide()
        {
            if (!_hidden)
            {
                Notifier.Hide(Toast);
            }

            _hidden = true;
        }
    }

    public sealed class UpdatableNotification<T>(ToastNotifierCompat notifier) : Notification(notifier) where T : Params
    {
        public void Update(T data)
            => Notifier.Update(data.CreateData(), Toast.Tag, Toast.Group);
    }

    public sealed class PromptNotification(ToastNotifierCompat notifier) : Notification(notifier);
}
