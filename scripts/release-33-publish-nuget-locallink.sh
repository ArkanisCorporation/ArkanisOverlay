#!/usr/bin/env bash
set -eEuo pipefail

### publishCmd
#
#| Command property | Description                                                                                                                                                                                                                                        |
#| ---------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
#| `exit code`      | Any non `0` code is considered as an unexpected error and will stop the `semantic-release` execution with an error.                                                                                                                                |
#| `stdout`         | The `release` information can be written to `stdout` as parseable JSON (for example `{"name": "Release name", "url": "http://url/release/1.0.0"}`). If the command write non parseable JSON to `stdout` no `release` information will be returned. |
#| `stderr`         | Can be used for logging.

[[ -z "${NUGET_PUBLISH_API_KEY+x}" ]] && echo "NUGET_PUBLISH_API_KEY is not set" && exit 2
[[ -z "${NUGET_PUBLISH_SOURCE_URL+x}" ]] && NUGET_PUBLISH_SOURCE_URL="https://api.nuget.org/v3/index.json"

>&2 echo "Pushing the LocalLink library NuGet package to the remote API..."
dotnet nuget push publish-nuget-locallink/* \
    --source ${NUGET_PUBLISH_SOURCE_URL} \
    --api-key ${NUGET_PUBLISH_API_KEY}

>&2 echo "Successfully published the LocalLink library NuGet package"
