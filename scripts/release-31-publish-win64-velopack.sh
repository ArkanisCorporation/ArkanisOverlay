#!/usr/bin/env bash
set -eEuo pipefail

### publishCmd
#
#| Command property | Description                                                                                                                                                                                                                                        |
#| ---------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
#| `exit code`      | Any non `0` code is considered as an unexpected error and will stop the `semantic-release` execution with an error.                                                                                                                                |
#| `stdout`         | The `release` information can be written to `stdout` as parseable JSON (for example `{"name": "Release name", "url": "http://url/release/1.0.0"}`). If the command write non parseable JSON to `stdout` no `release` information will be returned. |
#| `stderr`         | Can be used for logging.                                                                                                                                                                                                                           |

[[ -z "${VERSION_TAG+x}" ]] && >&2 echo "VERSION_TAG is not set" && exit 2
[[ -z "${VERSION_CHANNEL+x}" ]] && >&2 echo "VERSION_CHANNEL is not set" && exit 2
[[ -z "${GITHUB_TOKEN+x}" ]] && >&2 echo "GITHUB_TOKEN is not set" && exit 2
[[ -z "${REPOSITORY_URL+x}" ]] && REPOSITORY_URL="https://github.com/ArkanisCorporation/ArkanisOverlay"

[[ -d publish ]] || >&2 echo "publish directory does not exist" && exit 2
[[ -d release ]] || >&2 echo "release directory does not exist" && exit 2

>&2 echo "Uploading the packed application..."
dotnet vpk upload github \
    --repoUrl "${REPOSITORY_URL}" \
    --token "${GITHUB_TOKEN}" \
    --channel "${VERSION_CHANNEL}" \
    --outputDir release \
    --tag "${VERSION_TAG}" \
    --merge
