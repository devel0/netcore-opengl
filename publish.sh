#!/bin/bash

exdir=$(dirname `readlink -f "$0"`)

cd "$exdir"/netcore-opengl
rm -fr bin
dotnet pack -c Release
# dotnet nuget push bin/Release/*.nupkg -k $(cat ~/security/nuget-api.key) -s https://api.nuget.org/v3/index.json

cd "$exdir"
