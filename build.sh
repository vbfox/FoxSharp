#!/bin/bash

./paket.sh restore || { exit $?; }

dotnet run --project src/BlackFox.FoxSharp.Build/BlackFox.FoxSharp.Build.fsproj -- $@
