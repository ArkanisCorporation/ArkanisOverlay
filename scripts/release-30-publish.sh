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

run_sub "$THIS_DIR/release-31-publish-win64-velopack.sh"
run_sub "$THIS_DIR/release-32-publish-server.sh"
run_sub "$THIS_DIR/release-33-publish-nuget-common.sh"
run_sub "$THIS_DIR/release-34-publish-nuget-domain.sh"
run_sub "$THIS_DIR/release-35-publish-nuget-locallink.sh"
run_sub "$THIS_DIR/release-36-publish-nuget-external_common.sh"
