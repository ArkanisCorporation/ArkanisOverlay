#!/usr/bin/env bash
set -eEuo pipefail

### verifyReleaseCmd
#
#| Command property | Description                                                              |
#| ---------------- | ------------------------------------------------------------------------ |
#| `exit code`      | `0` if the verification is successful, or any other exit code otherwise. |
#| `stdout`         | Only the reason for the verification to fail can be written to `stdout`. |
#| `stderr`         | Can be used for logging.                                                 |

[[ -z "${VERSION+x}" ]] && echo "VERSION is not set" && exit 2
[[ -z "${VERSION_CHANNEL+x}" ]] && echo "VERSION_CHANNEL is not set" && exit 2
[[ -z "${GITHUB_TOKEN+x}" ]] && echo "GITHUB_TOKEN is not set" && exit 2
[[ -z "${REPOSITORY_URL+x}" ]] && REPOSITORY_URL="https://github.com/ArkanisCorporation/ArkanisOverlay"

if [[ ! -d publish-win64 ]]; then
  echo "publish directory does not exist"
  exit 2
fi

>&2 echo "Downloading previous release to build a delta release..."
dotnet vpk download github \
    --repoUrl "${REPOSITORY_URL}" \
    --token "${GITHUB_TOKEN}" \
    --channel "${VERSION_CHANNEL}" \
    --outputDir release-win64 \
    1>&2 # logging output must not go to stdout

>&2 echo "Packing the published application..."
dotnet vpk [win] pack \
    --packTitle "Arkanis Overlay" \
    --packId ArkanisOverlay \
    --packAuthors "FatalMerlin, TheKronnY, and contributors" \
    --splashImage ./src/Arkanis.Overlay.Host.Desktop/Resources/Logo/color_fill_blur_name_bg.png \
    --icon ./src/Arkanis.Overlay.Host.Desktop/Resources/favicon.ico \
    --packVersion "${VERSION}" \
    --framework net8.0-x64-desktop \
    --channel "${VERSION_CHANNEL}" \
    --packDir publish-win64 \
    --outputDir release-win64 \
    --mainExe ArkanisOverlay.exe \
    --signTemplate "jsign --storetype TRUSTEDSIGNING --keystore weu.codesigning.azure.net --storepass $AZURE_API_ACCESS_TOKEN --alias ArkanisOverlay/ArkanisOverlay {{file}}" \
    1>&2 # logging output must not go to stdout

>&2 echo "Successfully packed the Overlay application to: $(realpath release-win64)"
