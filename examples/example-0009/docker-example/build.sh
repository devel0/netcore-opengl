#!/bin/bash

dotnet build "$exdir"/..

exdir=$(dirname $(readlink -f "$0"))

docker build -t netcore-opengl-example-0009 "$exdir"
