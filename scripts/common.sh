#!/usr/bin/env bash
set -eEuo pipefail

function run_sub() {
    >&2 echo "running $1"
    if ! "$1"; then
        >&2 echo "failed running sub-script $1, exited with $?"
        exit 2
    fi
}
