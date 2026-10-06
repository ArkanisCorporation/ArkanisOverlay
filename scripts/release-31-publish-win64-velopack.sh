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
[[ -z "${GITHUB_REPOSITORY+x}" ]] && GITHUB_REPOSITORY="ArkanisCorporation/ArkanisOverlay"
[[ -z "${REPOSITORY_URL+x}" ]] && REPOSITORY_URL="https://github.com/${GITHUB_REPOSITORY}"

if [[ ! -d publish-win64 ]]; then
  >&2 echo "publish directory does not exist"
  exit 2
fi

if [[ ! -d release-win64 ]]; then
  >&2 echo "release directory does not exist"
  exit 2
fi

for attempt in {1..6} # wait for up to 1 minute
do
  if [[ $attempt -gt 1 ]]; then
    >&2 echo -e "\tWaiting for 10 seconds before retrying..."
    sleep 10
  fi

  >&2 echo "Checking that the GitHub release for ${VERSION_TAG} has been created... (attempt #$attempt)"
  if gh api "/repos/${GITHUB_REPOSITORY}/releases/tags/${VERSION_TAG}" >/dev/null; then
    >&2 echo -e "\tGitHub release for ${VERSION_TAG} exists!"
    break
  fi

  >&2 echo -e "\tGitHub release for ${VERSION_TAG} does not exist yet..."
done

>&2 echo "Uploading the packed application..."
dotnet vpk upload github \
    --repoUrl "${REPOSITORY_URL}" \
    --token "${GITHUB_TOKEN}" \
    --channel "${VERSION_CHANNEL}" \
    --outputDir release-win64 \
    --releaseName "${VERSION_TAG}" \
    --tag "${VERSION_TAG}" \
    --merge
