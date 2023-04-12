#!/bin/bash

exdir=$(dirname `readlink -f "$0"`)

modules="core gui shapes nurbs"

for module in $modules; do        
    rm -fr "$exdir"/src/$module/bin        
done

cd "$exdir"
dotnet pack -c Release

for module in $modules; do    
    ls "$exdir"/src/$module/bin/Release/*.nupkg
    
    dotnet nuget push "$exdir"/src/$module/bin/Release/*.nupkg \
        -k $(cat ~/security/nuget-api.key) -s https://api.nuget.org/v3/index.json \
        --skip-duplicate
done
