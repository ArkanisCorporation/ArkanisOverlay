#!/usr/bin/env bash
set -eEuo pipefail

### verifyReleaseCmd
#
#| Command property | Description                                                              |
#| ---------------- | ------------------------------------------------------------------------ |
#| `exit code`      | `0` if the verification is successful, or any other exit code otherwise. |
#| `stdout`         | Only the reason for the verification to fail can be written to `stdout`. |
#| `stderr`         | Can be used for logging.                                                 |

# --- Windows code-signing topology (Azure Artifact Signing, formerly Trusted Signing) ---
# WHERE we sign. These three values move together at a tenant/region/account cutover
# (see Infrastructure ARK-560); to repoint signing, edit them here - nothing else in this
# script hard-codes them. The per-tenant AZURE_* credentials are GitHub Actions secrets,
# injected via the environment (see .github/workflows/_release.yaml), not set here.
#   host    : account endpoint host, region-specific (neu = North Europe)
#   account : signing account name; the Jsign --alias is "<account>/<profile>"
#   profile : certificate profile name
SIGNING_ENDPOINT_HOST="neu.codesigning.azure.net"
SIGNING_ACCOUNT_NAME="ArkanisCorporationOU"
SIGNING_CERTIFICATE_PROFILE="ArkanisOverlay"

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

vpk_extra_params=()
if [[ $ENABLE_CODE_SIGNING == true ]]; then
    vpk_extra_params+=(--signTemplate "java -jar jsign-7.4.jar --storetype TRUSTEDSIGNING --keystore ${SIGNING_ENDPOINT_HOST} --storepass $AZURE_API_ACCESS_TOKEN --alias ${SIGNING_ACCOUNT_NAME}/${SIGNING_CERTIFICATE_PROFILE} {{file...}}")
fi

>&2 echo "Packing the published application..."
dotnet vpk [win] pack \
    --packTitle "Arkanis Overlay" \
    --packId ArkanisOverlay \
    --packAuthors "FatalMerlin, TheKronnY, and contributors" \
    --splashImage ./src/Arkanis.Overlay.Host.Desktop/Resources/Logo/color_fill_blur_name_bg.png \
    --icon ./src/Arkanis.Overlay.Host.Desktop/Resources/favicon.ico \
    --packVersion "${VERSION}" \
    --framework net10.0-x64-desktop \
    --channel "${VERSION_CHANNEL}" \
    --packDir publish-win64 \
    --outputDir release-win64 \
    --mainExe ArkanisOverlay.exe \
    "${vpk_extra_params[@]}" \
    1>&2 # logging output must not go to stdout

>&2 echo "Successfully packed the Overlay application to: $(realpath release-win64)"
