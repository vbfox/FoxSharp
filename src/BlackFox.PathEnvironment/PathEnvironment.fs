namespace BlackFox

open System
open System.IO

module private PathEnvironmentUtils =
    let private isWindows = Environment.OSVersion.Platform = PlatformID.Win32NT
    let private noExtensionsExecutable = not isWindows

    let private tryCombine (dir: string) (name: string) =
        try
            Some (Path.Combine(dir, name))
        with :? ArgumentException -> None

    let findFileInDirs dirs names =
        dirs
        |> Seq.collect (fun dir -> names |> List.choose (tryCombine dir))
        |> Seq.tryFind(File.Exists)

    let findProgramInDirs dirs programExts name =
        let namesWithExt = programExts |> List.map ((+) name)
        let names = if noExtensionsExecutable then name :: namesWithExt else namesWithExt
        findFileInDirs dirs names

    let private envVarOrEmpty name =
        let value = Environment.GetEnvironmentVariable name
        if isNull value then "" else value

    let private splitPathList (value: string) =
        value.Split Path.PathSeparator
        |> Array.filter (String.IsNullOrEmpty >> not)

    let getPath () =
        splitPathList (envVarOrEmpty "PATH")

    /// The fallback list embedded inside cmd.exe and used when PATHEXT is absent or empty.
    ///
    /// This value was extracted from the cmd.exe of a a Windows 11 Version 25H2 (OS Build 26200.9457)
    let private windowsFallbackPathExt = ".COM;.EXE;.BAT;.CMD;.VBS;.JS;.WS;.MSC"

    let getPathExt =
        if isWindows then
            fun () ->
                let rawEnvVar = envVarOrEmpty "PATHEXT"
                let rawPathExt = if rawEnvVar.Length = 0 then windowsFallbackPathExt else rawEnvVar
                splitPathList rawPathExt
        else
            fun () -> [|""|]

    let addCwd (includeCurrentDirectory: bool) (arr: string []) =
        if includeCurrentDirectory then
            Array.concat [ [|Environment.CurrentDirectory |]; arr ]
         else
            arr

open PathEnvironmentUtils

type PathEnvironment =
    /// Directories in the system PATH
    [<CompiledName("Path")>]
    static member path
        with get() = getPath()

    /// Extensions considered executables by the system.
    /// Parsed from `PATHEXT` on windows and always return `[|""|]` on other systems
    [<CompiledName("PathExt")>]
    static member pathExt
        with get() = getPathExt()

    /// Find an executable on the PATH
    [<CompiledName("FindExecutable")>]
    static member findExecutable (name: string) (includeCurrentDirectory: bool) =
        let dirs = addCwd includeCurrentDirectory PathEnvironment.path
        findProgramInDirs dirs (PathEnvironment.pathExt |> List.ofArray) name

    /// Find a file on the PATH
    [<CompiledName("FindFile")>]
    static member findFile (name: string) (includeCurrentDirectory: bool) =
        let dirs = addCwd includeCurrentDirectory PathEnvironment.path
        findFileInDirs dirs [name]
