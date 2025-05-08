#!/usr/bin/env bash
set -eEuo pipefail

### verifyReleaseCmd
#
#| Command property | Description                                                              |
#| ---------------- | ------------------------------------------------------------------------ |
#| `exit code`      | `0` if the verification is successful, or any other exit code otherwise. |
#| `stdout`         | Only the reason for the verification to fail can be written to `stdout`. |
#| `stderr`         | Can be used for logging.                                                 |

[[ -z "${CONFIGURATION+x}" ]] && CONFIGURATION="Release"

>&2 dotnet publish ./src/Arkanis.Overlay.Application/Arkanis.Overlay.Application.csproj \
    --runtime win-x64 \
    --configuration "${CONFIGURATION}" \
    --output publish \
    -p:EnableWindowsTargeting=true \
    -p:DebugType=None \
    -p:DebugSymbols=false
