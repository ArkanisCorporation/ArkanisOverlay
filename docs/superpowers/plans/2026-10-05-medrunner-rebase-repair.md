# MedRunner rebase repair implementation plan

> **For agentic workers:** Use superpowers:executing-plans to implement this plan in the current branch and checkout.

**Goal:** Rebase the MedRunner branch onto the locally available main snapshot without losing stable features or the user's existing edits.

**Architecture:** Replay the 39 commits after `d9e735e4` onto `fada1895`. Resolve conflicts against the current stable account architecture and dependency versions, retaining MedRunner additions. Verify the resulting feature delta and account behavior.

**Tech Stack:** Git, .NET, xUnit, bUnit.

**Spec:** The user's request in this chat: work fully locally in this branch and worktree, make proper backups, do not change remote branches, investigate and repair the rebase to preserve stable features.

## Global constraints

- Keep `feat/317-feat-add-medrunner-integration-rebased` in the existing checkout.
- Do not fetch, push, deploy, or modify any remote branch.
- Preserve the two pre-existing working edits byte-for-byte.
- Preserve stable `v1.10.2` and the local main snapshot, including account linking and refresh.
- Back up the original branch, index, working edits, and repository before replay.

## Review focus

- Citizen iD settings, Arkanis/RSI identity display, and refresh remain available.
- UEX and MedRunner linking coexist and imports still require consent.
- The shared-server account warning remains visible.
- Existing inventory/trade/search functionality and database migrations survive replay.
- Feature announcements and the user's MedRunner changes survive replay.

## Task 1: Preserve and repair the history

**Files:** Backup bundle, manifest, patches and working copies in `artifacts/rebase-repair/2026-10-05-before-repair`; conflicted tracked files selected by Git.

- [x] Inspect branches, worktrees, stable tag, merge bases, and rebase reflog.
- [x] Create and verify a local recovery branch and Git bundle; copy the index and working edits.
- [x] Run failing preservation checks for stable account files and stable ancestry.
- [x] Stash the user's two edits and record the stash object in the backup.
- [x] Replay the feature commits onto local main, resolving conflicts according to stable behavior and branch intent.
- [x] Check stable/main ancestry, absence of unresolved conflicts, and preservation of stable files.

## Task 2: Verify integrated behavior

**Files:** `tests/Arkanis.Overlay.Host.Desktop.UnitTests/Components/ExternalAccountControlsTests.cs`; conflict resolutions affecting account contexts, DI, preferences, and host projects.

- [x] Add behavior coverage for Citizen iD and MedRunner settings together, account identity display, and the shared-server warning.
- [x] Build and run local tests with live API calls disabled.
- [x] Inspect the entire feature delta against main, including deployment and migration files.
- [x] Restore the original user edits and verify their bytes against the backup.
- [x] Save the audit findings, validation results, and recovery instructions; leave all work local.
