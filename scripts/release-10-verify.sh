#!/usr/bin/env bash
set -eEuo pipefail

### verifyReleaseCmd
#
#| Command property | Description                                                              |
#| ---------------- | ------------------------------------------------------------------------ |
#| `exit code`      | `0` if the verification is successful, or any other exit code otherwise. |
#| `stdout`         | Only the reason for the verification to fail can be written to `stdout`. |
#| `stderr`         | Can be used for logging.                                                 |

[[ -n "${DEBUG}" ]] && env 1>&2

[[ -z "${VERSION+x}" ]] && echo "VERSION is not set" && exit 2
[[ -z "${VERSION_TAG+x}" ]] && echo "VERSION_TAG is not set" && exit 2

>&2 echo "Restoring .NET tools..."
dotnet tool restore 1>&2

>&2 echo "Applying the current release version ${VERSION} recursively..."
dotnet setversion --recursive "${VERSION}" 1>&2

"$(dirname "$(realpath "$0")")/release-11-verify-win64.sh"
"$(dirname "$(realpath "$0")")/release-12-verify-win64-velopack.sh"
"$(dirname "$(realpath "$0")")/release-13-verify-server.sh"
