# Contributing to Arkanis Overlay

Thank you for your interest in contributing to the Arkanis Overlay project!

We welcome contributions from the community and aim to maintain a clean and consistent codebase. Please review the following guidelines to understand how to effectively contribute to the project.

---

## Table of Contents

- [Getting Started](#getting-started)
- [AI Usage](#ai-usage)
- [Branching & Workflow](#branching--workflow)
- [Semantic Commits](#semantic-commits)
- [Code Style](#code-style)
- [Testing](#testing)
- [Pull Requests](#pull-requests)
- [Issue Tracking](#issue-tracking)
- [Project Structure](#project-structure)
- [CI/CD](#cicd)
- [Maintainers & Permissions](#maintainers--permissions)

---

## Getting Started

1. Fork the repository and clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/arkanis-overlay.git
   cd arkanis-overlay
   ```

2. Create a new branch from the latest `main`:
   ```bash
   git checkout -b feat/123-new-feature
   ```

3. Install [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) and verify:
   ```bash
   dotnet --version
   ```

---

## AI Usage

We recognize the utility of AI tools in software development.
While we appreciate the benevolent intentions behind AI-generated contributions, it often leads to issues such as:
* Inconsistent or very low code quality
* Incomplete or complete lack of understanding of the generated code
* Introduction of subtle bugs or security vulnerabilities
* Introduction of unmaintainable code
* Required time and effort to review, refactor, and test the generated code outweighing the benefits

Therefore, we allow the use of AI-assisted code generation tools ***only*** under the following conditions:
* You have reviewed the generated code in detail and understand it in its entirety.
* You ensure that the generated code adheres to our coding standards and project guidelines.
* You do not submit AI-generated code without proper review and testing.
* You disclose the use of AI tools in the PR description when applicable.

AI-powered auto-completion tools are permitted for code suggestions, provided the above conditions are met.
You do not need to disclose the use of such tools unless they generated significant portions of code.

---

## Branching & Workflow

We use a structured branch naming convention:

* Prefixes: `feat/`, `fix/`, `chore/`, `docs/`, `refactor/`, etc.
* Branches must begin with the related issue ID.

Example:

```bash
git checkout -b fix/123-resolve-login-bug
```

Branches prefixed with `release/` are protected and reserved for maintainers preparing official releases.

All changes are merged into `main` after review and CI validation.

---

## Semantic Commits

Please use [semantic commit messages](https://www.conventionalcommits.org/) to keep the history readable and informative.

**Format**:

```
<type>(<scope>): <short summary>
```

**Examples**:

```
feat(server): add new API endpoint for overlay config
fix(domain): resolve null ref in session manager
chore(build): update CI pipeline to .NET 8
```

Reference related issues in the commit body or footer when applicable.
Add body text for more context if needed.

---

## Code Style

* Code formatting is enforced via `.editorconfig`.
* Development is done primarily in JetBrains Rider. ReSharper-friendly changes are appreciated.
* Nullable reference types are enabled solution-wide.

---

## Testing

To run all tests locally:

```bash
dotnet test
```

**Guidelines**:

* Backend contributions (non-UI) should include tests where applicable.
* Follow existing test organization under `Arkanis.Overlay.*.UnitTests` and `Arkanis.Overlay.*.IntegrationTests`.
* You can read more in the testing-dedicated [README.md](./tests/README.md).

---

## Pull Requests

* All PRs must be reviewed by a maintainer before merging.
* Squashing commits is discouraged; we prefer maintaining full commit history.
* Reference the issue ID in your PR description with [closing keywords](https://docs.github.com/en/issues/tracking-your-work-with-issues/using-issues/linking-a-pull-request-to-an-issue) if applicable.
* Use the `WIP:` prefix for work-in-progress PRs, and remove it when ready for a review.
* Target the `main` branch, and ensure all CI checks pass before requesting a review.

---

## Issue Tracking

We use GitHub Issues to track bugs, possible enhancements, and feature requests.

* Opening an issue before submitting a PR is encouraged.
* Look for helpful labels like:
    * `good first issue`
    * `help wanted`

---

## Project Structure

The repository is organized as follows:

* `Arkanis.Overlay.Host.Desktop` – WPF host app
* `Arkanis.Overlay.Host.Server` – ASP.NET Core backend server
* `Arkanis.Overlay.Common`, `Components`, `Domain`, `Infrastructure`, etc. – shared libraries
* `Arkanis.Overlay.External.*` – integration with external systems

Use scoped commit messages where applicable, e.g.:

```
feat(server): add configuration endpoint
feat(external): add client SDK for external API of example.com
fix(components): resolve null reference in overlay component
```

---

## CI/CD

GitHub Actions validates all pull requests with:

* Build verification
* Test execution

CI runs on both internal and external (forked) PRs.

---

## Maintainers & Permissions

* All changes must go through a pull request, even for maintainers.
* Direct commits to `main` or `release/*` are restricted.
* In exceptional cases (e.g., emergencies), maintainers may bypass this process.

Long-term contributors may be invited to join the maintainer team.

---

Thank you for contributing to Arkanis Overlay!
If you have questions, feel free to open an issue or start a new discussion.
