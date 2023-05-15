#!/bin/bash
set -exo pipefail

cd "$(dirname ${BASH_SOURCE})"
cd ..

external_host_dir="${PWD}/gen"
mkdir -p "${external_host_dir}"

# build the image
docker build \
        --tag pano_tools:latest \
        --file Dockerfile.pano-converter .

# run the image
docker run \
        --rm \
        --interactive \
        --tty \
        --workdir /app \
        --env GEN_DIR=/gen \
        --user "$(id -u):$(id -g)" \
        --volume "${external_host_dir}:/gen" \
        pano_tools:latest \
        python3 "/app/src/convert.py" \
        "$@"
