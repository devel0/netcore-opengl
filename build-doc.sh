#!/bin/bash

exdir="$(dirname `readlink -f "$0"`)"

DOCSDIR="$exdir/docs"

rm -fr "$DOCSDIR"

mkdir "$DOCSDIR"

cd "$exdir"

doxygen

