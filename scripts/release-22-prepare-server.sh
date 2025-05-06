#!/usr/bin/env bash
set -eEuo pipefail

### prepareCmd
#
#| Command property | Description                                                                                                         |
#| ---------------- | ------------------------------------------------------------------------------------------------------------------- |
#| `exit code`      | Any non `0` code is considered as an unexpected error and will stop the `semantic-release` execution with an error. |
#| `stdout`         | Can be used for logging.                                                                                            |
#| `stderr`         | Can be used for logging.                                                                                            |

[[ -z "${VERSION+x}" ]] && >&2 echo "VERSION is not set" && exit 2
[[ -z "${VERSION_TAG+x}" ]] && >&2 echo "VERSION_TAG is not set" && exit 2
[[ -z "${REGISTRY+x}" ]] && REGISTRY="ghcr.io"
[[ -z "${CONFIGURATION+x}" ]] && CONFIGURATION="Release"

dotnet publish ./src/Arkanis.Overlay.Host.Server/Arkanis.Overlay.Host.Server.csproj \
    --configuration ${CONFIGURATION} \
    -p:PublishProfile=DefaultContainer \
    -p:ContainerRegistry=${REGISTRY} \
    -p:ContainerImageTag="${VERSION_TAG}"
