#!/bin/bash

./paket.sh restore || { exit $?; }

pushd src/BlackFox.FoxSharp.Build/
dotnet run $@
popd
