namespace BlackFox

open System
open System.IO

module private PathEnvironmentUtils =
    let private noExtensionsExecutable =
        match Environment.OSVersion.Platform with
        | PlatformID.Win32NT
        | PlatformID.Win32S
        | PlatformID.Win32Windows
        | PlatformID.WinCE
        | PlatformID.Xbox -> false
        | _ -> true

    let findFileInDirs dirs names =
        dirs
        |> Seq.collect (fun dir -> names |> List.map (fun name -> Path.Combine(dir, name)))
        |> Seq.tryFind(File.Exists)

    let findProgramInDirs dirs programExts name =
        let namesWithExt = programExts |> List.map ((+) name)
        let names = if noExtensionsExecutable then name :: namesWithExt else namesWithExt
        findFileInDirs dirs names

    let envVarOrEmpty name =
        let value = Environment.GetEnvironmentVariable(name)
        if isNull name then "" else value

    let getPath () =
        (envVarOrEmpty "PATH").Split(Path.PathSeparator)

    let getPathExt () =
        if Environment.OSVersion.Platform = PlatformID.Win32NT then
            (envVarOrEmpty "PATHEXT").Split(Path.PathSeparator)
        else
            [|""|]

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
