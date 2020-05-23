#!/bin/sh
if [ "$#" -ne 1 ]; then
  echo "Usage: $0 <version>"
  exit 1
fi

dotnet build --configuration Release -p:Version="$1"
dotnet pack --configuration Release --include-symbols -p:Version="$1"