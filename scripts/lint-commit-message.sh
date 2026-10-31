#!/usr/bin/env bash
# Husky.NET commit-msg check: rejects commit subjects that are not Conventional Commits.
#
# semantic-release (release.config.mjs) derives versions from commit types, so a malformed subject
# silently drops a change from the changelog or release. CONTRIBUTING.md documents the format;
# this hook enforces it locally. Git-generated subjects (merge, revert, fixup!/squash!) are allowed.
#
# Usage: lint-commit-message.sh <path-to-commit-message-file>
set -euo pipefail

message_file="${1:?usage: lint-commit-message.sh <commit-message-file>}"

# First line that is neither empty nor a git comment.
subject="$(grep -v -e '^#' -e '^[[:space:]]*$' "$message_file" | head -n 1 || true)"

types='build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test'
pattern="^((${types})(\([^()]+\))?!?: .+|Merge .+|Revert \".+\"|(fixup|squash|amend)! .+)$"

if [[ "$subject" =~ $pattern ]]; then
    exit 0
fi

{
    echo "Commit message subject does not follow Conventional Commits:"
    echo "  ${subject:-<empty>}"
    echo "Expected: <type>(<optional scope>)!: <summary>, with type one of: ${types//|/, }"
    echo "See CONTRIBUTING.md#semantic-commits."
} >&2
exit 1
