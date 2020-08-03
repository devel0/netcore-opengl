#!/bin/bash

exdir=$(dirname `readlink -f "$0"`)

find "$exdir"/docs -type f -exec sed -i -e "s#file:///Z:/netcore-opengl/##g" {} \;
