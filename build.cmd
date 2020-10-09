@echo off

call ./paket.cmd restore -s
if errorlevel 1 (
  exit /b %errorlevel%
)

dotnet run --project src\BlackFox.FoxSharp.Build\BlackFox.FoxSharp.Build.fsproj %*
