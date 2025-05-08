#!/usr/bin/env bash
set -eEuo pipefail

### verifyReleaseCmd
#
#| Command property | Description                                                              |
#| ---------------- | ------------------------------------------------------------------------ |
#| `exit code`      | `0` if the verification is successful, or any other exit code otherwise. |
#| `stdout`         | Only the reason for the verification to fail can be written to `stdout`. |
#| `stderr`         | Can be used for logging.                                                 |

[[ -z "${VERSION_TAG+x}" ]] && >&2 echo "VERSION_TAG is not set" && exit 2
[[ -z "${REGISTRY+x}" ]] && REGISTRY="ghcr.io"
[[ -z "${CONFIGURATION+x}" ]] && CONFIGURATION="Release"

dotnet publish ./src/Arkanis.Overlay.Host.Server/Arkanis.Overlay.Host.Server.csproj \
    --configuration "${CONFIGURATION}" \
    -p:PublishProfile=DefaultContainer \
    -p:ContainerRegistry="${REGISTRY}" \
    -p:ContainerImageTag="${VERSION_TAG}" \
    -p:DebugType=None \
    -p:DebugSymbols=false
