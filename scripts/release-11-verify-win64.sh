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

>&2 echo "Publishing the Windows Overlay application..."
dotnet publish ./src/Arkanis.Overlay.Host.Desktop/Arkanis.Overlay.Host.Desktop.csproj \
    --runtime win-x64 \
    --configuration "${CONFIGURATION}" \
    --output publish-win64 \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    1>&2 # logging output must not go to stdout

>&2 echo "Successfully published the Windows Overlay application to: $(realpath publish)"
