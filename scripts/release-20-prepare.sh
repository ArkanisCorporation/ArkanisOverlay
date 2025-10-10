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
#| `stderr`         | Can be used for logging.                     

[[ -z "${VERSION_TAG+x}" ]] && >&2 echo "VERSION_TAG is not set" && exit 2
[[ -z "${VERSION_CHANNEL+x}" ]] && >&2 echo "VERSION_CHANNEL is not set" && exit 2
[[ -z "${GITHUB_TOKEN+x}" ]] && >&2 echo "GITHUB_TOKEN is not set" && exit 2
[[ -z "${NUGET_PUBLISH_API_KEY+x}" ]] && echo "NUGET_PUBLISH_API_KEY is not set" && exit 2

DIRS=(
publish-win64
release-win64
publish-nuget-locallink
)
RETURN=0

for dir in "${DIRS[@]}"
do
    if [[ ! -d "${dir}" ]]; then
      >&2 echo "${dir} directory does not exist"
      RETURN=2
    fi
done

exit $RETURN
