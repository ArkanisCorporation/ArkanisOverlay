#!/usr/bin/env bash
set -eEuo pipefail

function build_nuget() {
    [[ -z "${CONFIGURATION+x}" ]] && CONFIGURATION="Release"

    local project_name="$1"
    local output_dir="$2"

    [[ -z "${project_name+x}" ]] && echo "project_name was not set for build_nuget()" && exit 2
    [[ -z "${output_dir+x}" ]] && echo "output_dir was not set for build_nuget()" && exit 2

    >&2 echo "Building ${project_name} library NuGet package..."
    dotnet pack "./src/${project_name}/${project_name}.csproj" \
        --configuration "${CONFIGURATION}" \
        --output "${output_dir}" \
        --include-symbols \
        --include-source \
        --no-restore \
        1>&2 # logging output must not go to stdout

    >&2 echo "Successfully built the ${project_name} library NuGet package"
}
