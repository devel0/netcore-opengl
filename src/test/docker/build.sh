#!/bin/bash

# in order to cache nuget package a tmp project refs containing all docker.sln references ( lib + test ) will generated

exdir=$(dirname $(readlink -f "$0"))

function scanprj() {
    if [ "$#" != "2" ]; then
        echo "invalid scanprj args"
        exit 1
    fi
    echo "SCAN PROJECT [$1]"
    readarray -t myarr < <(dotnet list "$1" package)

    if [ -e "$2" ]; then rm -f "$2"; fi

    for line in "${myarr[@]}"; do
        if [ "$(echo "$line" | grep '^[[:space:]]*>')" != "" ]; then
            pkgname="$(echo "$line" | awk '{print $2}')"
            pkgver="$(echo "$line" | awk '{print $3}')"
            echo "dotnet add package $pkgname --version $pkgver" >>$2
        fi
    done    

    cat "$2" | sort -u > "$2.tmp"
    mv -f "$2.tmp" "$2"
}

lib_pkgs="$exdir/.lib.pkgs"
refs_pkgs="$exdir/.refs.pkgs"
tmp_pkgs="$exdir/.tmp.pkgs"
prjroot="$exdir"/../../..

regen_refs=false

if [ ! -e "$exdir"/refs ]; then
    regen_refs=true
else
    scanprj "$prjroot"/src/test/docker/docker.sln "$lib_pkgs"

    scanprj "$prjroot"/src/test/docker/refs "$refs_pkgs"

    if [ "$(diff "$lib_pkgs" "$refs_pkgs")" != "" ]; then
        regen_refs=true
    fi
fi

regen_refs=false

if $regen_refs; then
    echo "REGEN REFS"

    rm -fr "$exdir"/refs
    dotnet new console -n refs
    cd refs

    scanprj "$prjroot"/src/test/docker/docker.sln "$tmp_pkgs"

    readarray -t myarr < <(cat "$tmp_pkgs" | sort -u)
    for line in "${myarr[@]}"; do "$($line)"; done

    rm -f "$tmp_pkgs"

    cd ..
fi

rm -f "$lib_pkgs"
rm -f "$refs_pkgs"

docker build \
    -t netcore-opengl-test \
    --build-arg USER_ID=$(id -u ${USER}) \
    --build-arg GROUP_ID=$(id -g ${USER}) \
    "$exdir"
