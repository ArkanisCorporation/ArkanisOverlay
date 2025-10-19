#!/usr/bin/env bash
set -eEuo pipefail

function publish_nuget() {
    [[ -z "${NUGET_PUBLISH_API_KEY+x}" ]] && >&2 echo "NUGET_PUBLISH_API_KEY is not set" && exit 2
    [[ -z "${NUGET_PUBLISH_SOURCE_URL+x}" ]] && NUGET_PUBLISH_SOURCE_URL="https://api.nuget.org/v3/index.json"

    local project_name="$1"
    local source_dir="$2"

    [[ -z "${project_name+x}" ]] && echo "project_name was not set for publish_nuget()" && exit 2
    [[ -z "${source_dir+x}" ]] && echo "source_dir was not set for publish_nuget()" && exit 2

    >&2 echo "Pushing the ${project_name} NuGet package to the remote API..."
    dotnet nuget push ${source_dir}/* \
        --source ${NUGET_PUBLISH_SOURCE_URL} \
        --api-key ${NUGET_PUBLISH_API_KEY} \
        --skip-duplicate

    >&2 echo "Successfully published the ${project_name} NuGet package"
}
