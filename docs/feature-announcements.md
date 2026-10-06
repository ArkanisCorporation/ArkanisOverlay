# Feature announcements and MedRunner availability

Feature announcements are registered in `src/Arkanis.Overlay.Components/Services/FeatureAnnouncementCatalog.cs`.
Create a Razor content component and add a catalog entry with a stable `Id`, `Title`, and `ContentComponentType`.
The shared dialog supplies the title and explicit **Got it — don't show again** button.

- Keep the ID stable. Increase `Revision` when existing users need to acknowledge materially new information.
- `Enabled = false` retires a temporary announcement without deleting its dismissal history.
- Optional UTC `AvailableFrom` (inclusive) and `AvailableUntil` (exclusive) restrict its display window.
- Higher `Priority` values display first; equal priorities use ordinal ID order.

`FeatureAnnouncementHost` is mounted once in `MainLayout`. The scoped service prevents concurrent queue runs.
Active notices are shown sequentially after rendering, and eligibility is checked again before each notice.
Interactive dialogs take precedence: opening another dialog cancels the notice and resumes the queue after the other dialog closes.
Backdrop clicks and Escape cannot acknowledge a notice. Host teardown and programmatic cancellation do not save a dismissal.

Each explicit dismissal is saved as `Id:Revision` in `UserPreferences.DismissedFeatureAnnouncements` using the existing preferences manager.
Desktop installations persist this in `userPreferences.json`; the server demo uses its existing in-memory preferences.
Older preference files omit this optional property and start with an empty set. The latest preferences are read at dismissal time to preserve settings changed while a notice was open.
Saving a preferences dialog also preserves the latest acknowledgment history from other windows.
Past dismissal keys are retained so re-enabling an unchanged notice does not show it again. Resetting application data resets this history.
Display windows are evaluated when the queue runs, not by a background timer.

The emergency announcement is temporary and can be retired by disabling its catalog entry. It accurately describes the current MedRunner portal handoff.
The emergency view's provider CTA opens the existing Arkanis Discord invite, where established providers can discuss offering services in the Overlay.

## MedRunner gate

`src/Arkanis.Overlay.Common/Options/MedRunnerIntegrationOptions.cs` defaults `AccountLinkingEnabled` to `false`.
The account context skips saved-credential validation, authentication, account/service refresh, and WebSocket initialization while disabled,
and rejects new account configuration. Local-link protocol credential imports for MedRunner are ignored before consent or token logging.
Existing saved credentials are retained for later re-enablement.
Settings hide linking and unlinking controls; setup dialogs display a portal notice; the emergency dialog shows the static portal card and omits in-app request steps.

To re-enable the integration, explicitly configure `MedRunnerIntegrationOptions.AccountLinkingEnabled = true` in the infrastructure registration,
review the existing live API setup and request-processing changes, and update the announcement's wording/revision as appropriate.
Focused tests cover disabled and explicitly enabled account contexts, portal-only rendering, queue ordering, persistence, interruption, and explicit dismissal.

## Verification while the desktop app is running

Build test artifacts separately to avoid locked desktop assemblies:

```powershell
dotnet test ArkanisOverlay.sln -p:ArtifactsPath="$PWD/artifacts/feature-announcements" -p:CopyLocalLockFileAssemblies=true
```

The copy-local option supplies test-runner dependencies that are otherwise absent in this branch's shared artifacts output.
LocalLink named-pipe tests use the application's pipe name and may fail with `All pipe instances are busy` while a desktop instance is running.
