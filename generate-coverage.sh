#!/bin/bash
exdir="$(dirname `readlink -f "$0"`)"
cd "$exdir"
dotnet test src/test/test.csproj \
	/p:CollectCoverage=true /p:CoverletOutputFormat=\"opencover,lcov\" /p:CoverletOutput=../../lcov
