#!/usr/bin/env bash
set -eEuo pipefail

THIS_DIR="$(dirname "$(realpath "$0")")"

. "${THIS_DIR}/common-verify.sh"

### verifyReleaseCmd
#
#| Command property | Description                                                              |
#| ---------------- | ------------------------------------------------------------------------ |
#| `exit code`      | `0` if the verification is successful, or any other exit code otherwise. |
#| `stdout`         | Only the reason for the verification to fail can be written to `stdout`. |
#| `stderr`         | Can be used for logging.                                                 |

# calling shared function from common-verify.sh
build_nuget "Arkanis.Overlay.LocalLink" "publish-nuget-locallink"
