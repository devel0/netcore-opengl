#!/bin/env bash

exdir=$(dirname $(readlink -f "$0"))

docker run \
    -v "$exdir/../../..:/netcore-opengl" \
    -e DOTNET_CLI_HOME=/home/user \
    -e XDG_DATA_HOME=/home/user \
    netcore-opengl-test \
    su -c "bash -c 'xvfb-run dotnet test /netcore-opengl/src/test/test.csproj'" user
