#!/bin/bash

CDIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd $CDIR

rimraf ../data/test-extensions/Sample.Extension

# see: https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish
dotnet publish -f net5.0 -r win10-x64 -c Development --no-dependencies --no-self-contained -o ../data/test-extensions/Sample.Extension