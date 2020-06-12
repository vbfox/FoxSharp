# Visual Studio 2017+ Locator

![Logo '?VS'](https://raw.githubusercontent.com/vbfox/FoxSharp/master/src/BlackFox.VsWhere/Icon.png)

[![Nuget Package](https://img.shields.io/nuget/v/BlackFox.VsWhere.svg)](https://www.nuget.org/packages/BlackFox.VsWhere)

A NuGet package to detect Visual Studio 2017+ installs from F# code similar to the
[vswhere](https://github.com/Microsoft/vswhere) binary.

*Note: The API is also usable in other .NET languages but exposes some F# specific types*

## API

```fsharp
module BlackFox.VsWhere.VsInstances =
    /// Get all VS2017+ instances (Visual Studio stable, preview, Build tools, ...)
    /// This method return instances that have installation errors and pre-releases.
    let getAll (): VsSetupInstance list =

    /// Get VS2017+ instances that are completely installed
    let getCompleted (includePrerelease: bool): VsSetupInstance list =

    /// Get VS2017+ instances that are completely installed and have a specific package ID installed
    let getWithPackage (packageId: string) (includePrerelease: bool): VsSetupInstance list =

    /// Get legacy VS instances (before VS2017: VS .NET 2002 to VS2015).
    /// Note that the information for legacy ones is limited.
    let getLegacy(): VsSetupInstance list =

    /// Get all Visual Studio instances including legacy VS instances (before VS2017: VS .NET 2002 to VS2015).
    /// Note that the information for legacy ones is limited.
    let getAllWithLegacy (): VsSetupInstance list =
```

Package IDs for components and workloads [are documented on docs.microsoft.com](https://docs.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-build-tools?view=vs-2017).

## Sample usage

Find MSBuild:

```fsharp
let instance =
    VsInstances.getWithPackage "Microsoft.Component.MSBuild" false
    |> List.tryHead

match instance with
| None -> printfn "No MSBuild"
| Some vs ->
    let msbuild = Path.Combine(vs.InstallationPath, "MSBuild\\15.0\\Bin\\MSBuild.exe")
    printfn "MSBuild: %s" msbuild
```

Start Developer Command Prompt:

```fsharp
match VsInstances.getCompleted false |> List.tryHead with
| None -> printfn "No VS"
| Some vs ->
    let vsdevcmd = Path.Combine(vs.InstallationPath, "Common7\\Tools\\vsdevcmd.bat")
    let comspec = Environment.GetEnvironmentVariable("COMSPEC")
    Process.Start(comspec, sprintf "/k \"%s\"" vsdevcmd) |> ignore
```
