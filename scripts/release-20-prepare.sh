#!/usr/bin/env bash
set -eEuo pipefail

THIS_DIR="$(dirname "$(realpath "$0")")"

. "${THIS_DIR}/common.sh"

### prepareCmd
#
#| Command property | Description                                                                                                         |
#| ---------------- | ------------------------------------------------------------------------------------------------------------------- |
#| `exit code`      | Any non `0` code is considered as an unexpected error and will stop the `semantic-release` execution with an error. |
#| `stdout`         | Can be used for logging.                                                                                            |
#| `stderr`         | Can be used for logging.                                                                                            |

exit 0
