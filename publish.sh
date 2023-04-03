#!/bin/bash

exdir=$(dirname `readlink -f "$0"`)

cd "$exdir"
dotnet pack -c Release

for module in core render gui shapes; do
    ls "$exdir"/src/$module/bin/Release/*.nupkg
    
    dotnet nuget push "$exdir"/src/$module/bin/Release/*.nupkg \
        -k $(cat ~/security/nuget-api.key) -s https://api.nuget.org/v3/index.json
done
