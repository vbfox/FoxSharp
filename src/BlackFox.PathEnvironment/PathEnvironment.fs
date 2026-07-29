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

    let envVarOrEmpty name =
        let value = Environment.GetEnvironmentVariable(name)
        if isNull value then "" else value

    let private splitPathList (value: string) =
        value.Split(Path.PathSeparator)
        |> Array.filter (String.IsNullOrEmpty >> not)

    let getPath () =
        splitPathList (envVarOrEmpty "PATH")

    let getPathExt () =
        if Environment.OSVersion.Platform = PlatformID.Win32NT then
            splitPathList (envVarOrEmpty "PATHEXT")
        else
            [|""|]

    let addCwd (includeCurrentDirectory: bool) (arr: string []) =
        if includeCurrentDirectory then
            Array.concat [ [|Environment.CurrentDirectory |]; arr ]
         else
            arr

open PathEnvironmentUtils

type PathEnvironment =
    /// Directories in the system PATH.
    /// Empty entries are removed, they would otherwise be resolved against the current directory.
    [<CompiledName("Path")>]
    static member path
        with get() = getPath()

    /// Extensions considered executables by the system.
    /// Parsed from `PATHEXT` on windows and always return `[|""|]` on other systems
    [<CompiledName("PathExt")>]
    static member pathExt
        with get() = getPathExt()

    /// <summary>Find an executable on the PATH</summary>
    /// <remarks>
    /// <para><c>name</c> is combined with each directory as a relative path, a <c>name</c> that contains directory
    /// separators or <c>..</c> can therefore designate a file outside of the PATH directories. Don't pass a name
    /// coming from an untrusted source without validating it first.</para>
    /// <para>When <c>includeCurrentDirectory</c> is <c>true</c> the current directory is searched
    /// before the PATH, as Windows does when starting a process. A file dropped in the current
    /// directory then takes precedence over the system one, only enable it when that is what you want.</para>
    /// </remarks>
    [<CompiledName("FindExecutable")>]
    static member findExecutable (name: string) (includeCurrentDirectory: bool) =
        let dirs = addCwd includeCurrentDirectory PathEnvironment.path
        findProgramInDirs dirs (PathEnvironment.pathExt |> List.ofArray) name

    /// <summary>Find a file on the PATH</summary>
    /// <remarks>
    /// <para><c>name</c> is combined with each directory as a relative path, a <c>name</c> that contains directory
    /// separators or <c>..</c> can therefore designate a file outside of the PATH directories. Don't pass a name
    /// coming from an untrusted source without validating it first.</para>
    /// <para>When <c>includeCurrentDirectory</c> is <c>true</c> the current directory is searched
    /// before the PATH.</para>
    /// </remarks>
    [<CompiledName("FindFile")>]
    static member findFile (name: string) (includeCurrentDirectory: bool) =
        let dirs = addCwd includeCurrentDirectory PathEnvironment.path
        findFileInDirs dirs [name]
