#!/usr/bin/env bash
set -eEuo pipefail

### publishCmd
#
#| Command property | Description                                                                                                                                                                                                                                        |
#| ---------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
#| `exit code`      | Any non `0` code is considered as an unexpected error and will stop the `semantic-release` execution with an error.                                                                                                                                |
#| `stdout`         | The `release` information can be written to `stdout` as parseable JSON (for example `{"name": "Release name", "url": "http://url/release/1.0.0"}`). If the command write non parseable JSON to `stdout` no `release` information will be returned. |
#| `stderr`         | Can be used for logging.

[[ -z "${VERSION_TAG+x}" ]] && >&2 echo "VERSION_TAG is not set" && exit 2
[[ -z "${GITHUB_REPOSITORY+x}" ]] && GITHUB_REPOSITORY="ArkanisCorporation/ArkanisOverlay"
[[ -z "${REGISTRY+x}" ]] && REGISTRY="ghcr.io"
[[ -z "${CONFIGURATION+x}" ]] && CONFIGURATION="Release"

DOCKERFILE_PATH=./src/Arkanis.Overlay.Host.Server/Dockerfile
IMAGE_NAME_BARE=$(echo "${GITHUB_REPOSITORY}" | tr '[:upper:]' '[:lower:]')
IMAGE_NAME=$(echo "${REGISTRY}/${IMAGE_NAME_BARE}")

IMAGE_NAME_VERSION_TAGGED="${IMAGE_NAME}:${VERSION_TAG}"
IMAGE_NAME_CHANNEL_TAGGED="${IMAGE_NAME}:${VERSION_CHANNEL}-latest"
IMAGE_NAME_LATEST_TAGGED="${IMAGE_NAME}:latest"

>&2 echo "Publishing ${IMAGE_NAME}..."
>&2 echo -e "\tas ${IMAGE_NAME_VERSION_TAGGED}"
>&2 echo -e "\tas ${IMAGE_NAME_CHANNEL_TAGGED}"

docker buildx build \
    --push \
    --cache-to type=gha \
    --tag "${IMAGE_NAME}:${VERSION_TAG}" \
    --tag "${IMAGE_NAME}:latest" \
    --file "${DOCKERFILE_PATH}" \
    --build-arg BUILD_CONFIGURATION=${CONFIGURATION} \
    . \
    1>&2 # logging output must not go to stdout

if [[ "${VERSION_CHANNEL}" == "stable" ]]; then
    >&2 echo -e "\tas ${IMAGE_NAME_LATEST_TAGGED}"
    docker buildx build \
        --push \
        --tag "${IMAGE_NAME_LATEST_TAGGED}" \
        --file "${DOCKERFILE_PATH}" \
        --build-arg BUILD_CONFIGURATION=${CONFIGURATION} \
        . \
        1>&2 # logging output must not go to stdout
fi

>&2 echo "Successfully published ${IMAGE_NAME}!"
