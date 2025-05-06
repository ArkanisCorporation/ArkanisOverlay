#!/usr/bin/env bash
set -eEuo pipefail

### verifyReleaseCmd
#
#| Command property | Description                                                              |
#| ---------------- | ------------------------------------------------------------------------ |
#| `exit code`      | `0` if the verification is successful, or any other exit code otherwise. |
#| `stdout`         | Only the reason for the verification to fail can be written to `stdout`. |
#| `stderr`         | Can be used for logging.                                                 |

[[ -z "${VERSION+x}" ]] && >&2 echo "VERSION is not set" && exit 2
[[ -z "${VERSION_TAG+x}" ]] && >&2 echo "VERSION_TAG is not set" && exit 2

dotnet tool restore
dotnet setversion --recursive "${VERSION}"
