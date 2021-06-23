#!/bin/bash

exdir="$(dirname `readlink -f "$0"`)"

DOCSDIR="$exdir/../docs"

rm -fr "$DOCSDIR"

cd "$exdir"

docfx

rsync -arvx "$exdir/../test/" "$DOCSDIR/test/" \
    --exclude=bin \
    --exclude=obj

mkdir -p "$DOCSDIR/data/techdocs"

rsync -arvx "$exdir/../data/techdocs/" "$DOCSDIR/data/techdocs/" \
    --exclude=bin \
    --exclude=obj
