#!/bin/bash

exdir=$(dirname `readlink -f "$0"`)

rm -f "$exdir"/output/example-0009.png

dotnet clean "$exdir"/..
dotnet build "$exdir"/..

docker run \
    -v "$exdir/../../..:/netcore-opengl" \
    -v "$exdir/output:/root/Desktop" \
    netcore-opengl-example-0009 \
    bash -c "xvfb-run dotnet /netcore-opengl/examples/example-0009/bin/Debug/net7.0/example-0009.dll && echo finished && chown -R $(id -u).$(id -g) /root/Desktop"

echo "Generated file:"

ls -lah "$exdir"/output