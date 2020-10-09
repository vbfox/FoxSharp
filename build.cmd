@echo off

./paket.cmd restore -s
dotnet run --project src\BlackFox.FoxSharp.Build\BlackFox.FoxSharp.Build.fsproj %*
