module BlackFox.PathEnvironment

open System
open System.IO

module private Utils =
    let findFileInDirs dirs names =
        dirs
        |> Seq.collect (fun dir -> names |> List.map (fun name -> Path.Combine(dir, name)))
        |> Seq.tryFind(File.Exists)

    let findProgramInDirs dirs programExts name =
        let names = name :: (programExts |> List.map ((+) name))
        findFileInDirs dirs names

    let envVarOrEmpty name =
        let value = Environment.GetEnvironmentVariable(name)
        if isNull name then "" else value

    let getPath () =
        (envVarOrEmpty "PATH").Split(Path.PathSeparator) |> List.ofArray

    let getPathExt () =
        if Environment.OSVersion.Platform = PlatformID.Win32NT then
            (envVarOrEmpty "PATHEXT").Split(Path.PathSeparator) |> List.ofArray
        else
            [""]

open Utils

/// Directories in the system PATH
let path =
    lazy (getPath())

/// Extensions considered executables by the system.
/// Parsed from PATHEXT on windows and always return [""] on other systems
let pathExt =
    lazy (getPathExt())

/// Find an executable in PATH (Extension is optional if present in PATHEXT)
let findInPathOnly name =
    findProgramInDirs path.Value pathExt.Value name

/// Find an executable in the current directory or PATH (Extension is optional if present in PATHEXT)
let find name =
    findProgramInDirs (Environment.CurrentDirectory :: path.Value) pathExt.Value name
