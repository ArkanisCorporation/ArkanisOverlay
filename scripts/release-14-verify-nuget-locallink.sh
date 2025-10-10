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

>&2 echo "Building LocalLink library NuGet package..."
dotnet pack ./src/Arkanis.Overlay.LocalLink/Arkanis.Overlay.LocalLink.csproj \
    --configuration "${CONFIGURATION}" \
    --output publish-nuget-locallink \
    --include-symbols \
    --include-source \
    --no-restore \
    1>&2 # logging output must not go to stdout

>&2 echo "Successfully built the LocalLink library NuGet package"
