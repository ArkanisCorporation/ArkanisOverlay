#!/usr/bin/env bash
set -eEuo pipefail

THIS_DIR="$(dirname "$(realpath "$0")")"

. "${THIS_DIR}/common.sh"

### verifyReleaseCmd
#
#| Command property | Description                                                              |
#| ---------------- | ------------------------------------------------------------------------ |
#| `exit code`      | `0` if the verification is successful, or any other exit code otherwise. |
#| `stdout`         | Only the reason for the verification to fail can be written to `stdout`. |
#| `stderr`         | Can be used for logging.                                                 |

[[ -n "${DEBUG+x}" ]] && env 1>&2

[[ -z "${VERSION+x}" ]] && echo "VERSION is not set" && exit 2
[[ -z "${VERSION_TAG+x}" ]] && echo "VERSION_TAG is not set" && exit 2

>&2 echo "Restoring .NET tools..."
dotnet tool restore 1>&2 # logging output must not go to stdout

>&2 echo "Applying the current release version ${VERSION} recursively..."
dotnet setversion --recursive "${VERSION}" 1>&2 # logging output must not go to stdout

run_sub "$THIS_DIR/release-11-verify-win64.sh"
run_sub "$THIS_DIR/release-12-verify-win64-velopack.sh"
run_sub "$THIS_DIR/release-13-verify-server.sh"
run_sub "$THIS_DIR/release-14-verify-nuget-locallink.sh"
