#!/usr/bin/env bash
set -eEuo pipefail

### verifyReleaseCmd
#
#| Command property | Description                                                              |
#| ---------------- | ------------------------------------------------------------------------ |
#| `exit code`      | `0` if the verification is successful, or any other exit code otherwise. |
#| `stdout`         | Only the reason for the verification to fail can be written to `stdout`. |
#| `stderr`         | Can be used for logging.                                                 |

[[ -z "${VERSION_TAG+x}" ]] && echo "VERSION_TAG is not set" && exit 2
[[ -z "${GITHUB_REPOSITORY+x}" ]] && GITHUB_REPOSITORY="ArkanisCorporation/ArkanisOverlay"
[[ -z "${REGISTRY+x}" ]] && REGISTRY="ghcr.io"
[[ -z "${CONFIGURATION+x}" ]] && CONFIGURATION="Release"

IMAGE_NAME_BARE=$(echo "${GITHUB_REPOSITORY}" | tr '[:upper:]' '[:lower:]')
IMAGE_NAME=$(echo "${REGISTRY}/${IMAGE_NAME_BARE}")

>&2 echo "Building ${IMAGE_NAME_BARE}..."
docker buildx build \
    --load \
    --cache-from type=gha \
    --cache-from "${IMAGE_NAME}" \
    --cache-to type=inline,mode=max \
    --tag "${IMAGE_NAME_BARE}" \
    --file ./src/Arkanis.Overlay.Host.Server/Dockerfile \
    --build-arg BUILD_CONFIGURATION=${CONFIGURATION} \
    . \
    1>&2 # logging output must not go to stdout

>&2 echo "Successfully built ${IMAGE_NAME_BARE}!"
