# Visual Studio 2017+ Locator

[![Nuget Package](https://img.shields.io/nuget/v/BlackFox.VsWhere.svg)](https://www.nuget.org/packages/BlackFox.VsWhere)

A NuGet package to detect Visual Studio 2017+ installs from F# code similar to the
[vswhere](https://github.com/Microsoft/vswhere) binary.

*Note: The API is also usable in other .NET languages but exposes some F# specific types*

## API

```fsharp
module BlackFox.VsWhere.VsInstances =
    let getAll (): VsSetupInstance [] =
    let getWithPackage (packageId: string) (includePrerelease: bool): VsSetupInstance [] =
```

## Sample usage

Find MSBuild:

```fsharp
let instance =
    VsInstances.getWithPackage "Microsoft.Component.MSBuild" false
    |> Array.tryHead

match instance with
| None -> printfn "No MSBuild"
| Some vs ->
    let msbuild = Path.Combine(vs.InstallationPath, "MSBuild\\15.0\\Bin\\MSBuild.exe")
    printfn "MSBuild: %s" msbuild
```

Start Developer Command Prompt

```fsharp
match VsInstances.getCompleted false |> Array.tryHead with
| None -> printfn "No VS"
| Some vs ->
    let vsdevcmd = Path.Combine(vs.InstallationPath, "Common7\\Tools\\vsdevcmd.bat")
    let comspec = Environment.GetEnvironmentVariable("COMSPEC")
    Process.Start(comspec, sprintf "/k \"%s\"" vsdevcmd) |> ignore
```
