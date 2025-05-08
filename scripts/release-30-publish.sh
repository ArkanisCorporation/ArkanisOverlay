#!/usr/bin/env bash
set -eEuo pipefail

THIS_DIR="$(dirname "$(realpath "$0")")"

. "${THIS_DIR}/common.sh"

### publishCmd
#
#| Command property | Description                                                                                                                                                                                                                                        |
#| ---------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
#| `exit code`      | Any non `0` code is considered as an unexpected error and will stop the `semantic-release` execution with an error.                                                                                                                                |
#| `stdout`         | The `release` information can be written to `stdout` as parseable JSON (for example `{"name": "Release name", "url": "http://url/release/1.0.0"}`). If the command write non parseable JSON to `stdout` no `release` information will be returned. |
#| `stderr`         | Can be used for logging.                                                                                                                                                                                                                           |

run_sub "$(dirname "$(realpath "$0")")/release-31-publish-win64-velopack.sh"
