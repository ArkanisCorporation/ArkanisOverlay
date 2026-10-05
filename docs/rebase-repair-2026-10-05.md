# Local MedRunner rebase repair — 2026-10-05

The MedRunner branch now includes the locally available main history, including stable `v1.10.2`, while retaining its feature commits. Work was performed in the existing branch and checkout. No fetch, push, remote branch update, or deployment was performed.

## Evidence and cause

| Reference | Commit |
| --- | --- |
| Branch before repair | `2327e94940ad4cd96f3858fd1c3d6a513991bc13` |
| Previous rebase base | `d9e735e4790e571850f5ac01275f52491deacf34` |
| Local main snapshot used for repair | `fada18951b22f08a71044ac59192b5c1187463ba` |
| Latest locally available stable tag, `v1.10.2` | `ed7e53f3c0a91137021407b5dfe8fee61b2787ce` |
| Replayed feature tip before integration corrections | `f021f24a` |

The saved branch reflog records the previous rebase finishing on October 16, 2025, onto `d9e735e4`. Later feature work continued on that base. There were 408 main commits between that base and the local main snapshot. The original branch did not contain the stable Citizen iD implementation or the subsequent account refresh architecture. The branch name alone therefore did not establish that it had been rebased onto recent stable changes.

All 39 feature commits after the old base were replayed onto `fada1895`. Stable ancestry and required account files failed 13 preservation checks before replay and passed afterward. This investigation uses local refs and tags; remote freshness was deliberately not checked.

## Corrections and preservation decisions

- Combined the real Citizen iD settings with MedRunner controls. Preserved Arkanis/RSI identity display, Citizen iD linking, and the shared-server warning.
- Preserved stable authenticator service identifiers, required credential JSON fields, refresh subscriptions, and disposal cleanup. Adapted the Citizen iD identity update override to the feature branch's renamed authentication-state hook.
- Registered the MedRunner authenticator with the shared provider so link consent can select it. Registered its account context through the shared account-context abstraction.
- Removed duplicate preference-manager and initialization-host registrations introduced while combining the two branches. Retained stable persistence choices for local and server hosting.
- Kept stable solution projects, .NET 10, and existing package versions. Ported the new MedRunner project to .NET 10, added matching feature dependencies, and updated the branch's bUnit tests to bUnit 2.11.3.
- Removed the case-colliding feature `NuGet.Config` and restored stable `nuget.config`, including source mappings. Regenerated lock files from cached packages; existing resolved stable packages were retained or advanced to satisfy the added dependencies.
- Preserved stable UEX schema and generated client signatures. Their feature delta consists of generator version annotations.
- Restored stable marketplace-provider registration and removed the terminal 778 exclusion. These unrelated changes originated in feature commit `578ca2d0`, not in conflict resolution. Restoring registration preserves stable behavior; it does not implement the already-incomplete marketplace DTO mapper.
- Kept stable migrations, AppHost and Kubernetes configuration, deployment workflows, and the root SDK pin unchanged. Retained MedRunner dialogs, announcements, location support, and shared UI refinements.

A separate read-only review checked account lifecycle, consent, preference persistence, host lifecycle, dependencies, UEX contracts, inventory/trade migrations, and deployment preservation. It found no additional actionable rebase regression.

## Validation

The final working tree, including the user's original edits, built successfully. All **116 non-live tests passed**:

| Test project | Passed |
| --- | ---: |
| Infrastructure | 57 |
| Desktop | 36 |
| Aspire | 15 |
| Server | 5 |
| LocalLink | 3 |

New behavior tests cover Citizen iD and MedRunner settings together in both hosting modes, enabled/disabled MedRunner linking, Citizen iD/RSI identity display, refresh and unlink, authenticator selection for consent, and preference-manager registration. Existing UI tests were ported to bUnit 2; the fallback test now checks the provider inquiry link actually shown in the first dialog step.

Verification stayed offline. Restore used the installed package cache as its only source. Builds compiled the checked-in API clients without invoking tool restore or client regeneration. Tests used `DataState!=Live`; Aspire tests generated fixture manifests locally without deploying them. Live authentication, external API compatibility, and interactive desktop behavior were not exercised.

The repository pins SDK 10.0.301, which is unavailable on this machine. Verification used installed SDK 10.0.302, temporarily selected for child-process tests, and restored `global.json` byte-for-byte afterward. Build warnings remain for SQLitePCLRaw 2.1.11, the UI test dependency System.Drawing.Common 4.7.0, and the existing Aspire CLI bundle setting.

The backup directory contains `validation/verify-local.ps1`, offline configuration/target overrides, build logs, and TRX results. Final logs are `confirmed-final-build.log` and `confirmed-final-tests.log`.

## Backups and recovery

Backups are kept under `artifacts/rebase-repair/2026-10-05-before-repair/` in this checkout:

- Local recovery branch: `backup/medrunner-before-rebase-repair-2026-10-05`.
- Verified complete `repository.bundle`, binary working/index patches, original index, refs, status, and branch reflog.
- Exact original copies of the two modified working files under `working-files/`.
- Recorded stash object in `user-edits-stash.txt` and a separate `user-edits.bundle`. The stash was applied and retained.

The original edits to `MedRunnerIntegrationOptions.cs` and `ApiEndpoint.cs` were restored and matched their saved SHA-256 hashes. They remain uncommitted and are excluded from the repair commit.

The recovery branch preserves the original committed history without changing the repaired checkout. To inspect it separately, use `git worktree add --detach <new-local-directory> backup/medrunner-before-rebase-repair-2026-10-05`. The patches and exact file copies preserve the original uncommitted state. Do not reset the repaired checkout to inspect the backup.

Local main, remote-tracking refs, and other worktrees were preserved. The repaired branch has rewritten local feature history; its remote branch retains the original commits.
