module BlackFox.FoxSharp.Tests.PathEnvironmentTests

open System
open System.IO
open Expecto
open Expecto.Flip
open BlackFox

/// These tests mutate process-wide state (PATH/PATHEXT env vars and the current directory)
/// so they must not run concurrently with each other or with other tests.
let private withEnvVar (name: string) (value: string) (f: unit -> unit) =
    let previous = Environment.GetEnvironmentVariable(name)
    try
        Environment.SetEnvironmentVariable(name, value)
        f ()
    finally
        Environment.SetEnvironmentVariable(name, previous)

let private withCurrentDirectory (dir: string) (f: unit -> unit) =
    let previous = Environment.CurrentDirectory
    try
        Environment.CurrentDirectory <- dir
        f ()
    finally
        Environment.CurrentDirectory <- previous

let private isWindows = Environment.OSVersion.Platform = PlatformID.Win32NT

let private mkTmpDir () =
    Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "BlackFox.PathEnvironment.Tests_" + Guid.NewGuid().ToString("N")))

let private withTmpDir (f: DirectoryInfo -> unit) =
    let tmpDir = mkTmpDir ()
    try
        f tmpDir
    finally
        Directory.Delete(tmpDir.FullName, true)

let private touch (path: string) =
    File.WriteAllText(path, "")

[<Tests>]
let tests =
    testSequenced <| testList "PathEnvironment" [
        testCase "findFile with includeCurrentDirectory=false doesn't search the CWD because of a trailing PATH separator" <| fun () ->
            withTmpDir (fun root ->
                let cwd = Directory.CreateDirectory(Path.Combine(root.FullName, "cwd"))
                let emptyPathDir = Directory.CreateDirectory(Path.Combine(root.FullName, "on-path"))
                let markerName = "marker_" + Guid.NewGuid().ToString("N") + ".txt"
                File.WriteAllText(Path.Combine(cwd.FullName, markerName), "")

                // A trailing separator (common on Windows) produces an empty entry once PATH is split.
                let pathWithTrailingSeparator = emptyPathDir.FullName + string Path.PathSeparator

                withEnvVar "PATH" pathWithTrailingSeparator (fun () ->
                    withCurrentDirectory cwd.FullName (fun () ->
                        let found = PathEnvironment.findFile markerName false
                        Expect.equal "The current directory must not be searched" None found
                    )
                )
            )

        testCase "findFile still finds a directory listed on PATH when there is also a trailing separator" <| fun () ->
            withTmpDir (fun pathDir ->
                let markerName = "marker_" + Guid.NewGuid().ToString("N") + ".txt"
                let markerPath = Path.Combine(pathDir.FullName, markerName)
                File.WriteAllText(markerPath, "")

                let pathWithTrailingSeparator = pathDir.FullName + string Path.PathSeparator

                withEnvVar "PATH" pathWithTrailingSeparator (fun () ->
                    let found = PathEnvironment.findFile markerName false
                    Expect.equal "The file on PATH must still be found" (Some markerPath) found
                )
            )

        testCase "path doesn't throw and is empty when PATH is unset" <| fun () ->
            withEnvVar "PATH" null (fun () ->
                Expect.equal "" [||] PathEnvironment.path
            )

        testCase "pathExt falls back to the cmd.exe built-in list when PATHEXT is unset (Windows only)" <| fun () ->
            if not isWindows then skiptest "PATHEXT is only used on Windows"

            withEnvVar "PATHEXT" null (fun () ->
                let cmdBuiltInList = [| ".COM"; ".EXE"; ".BAT"; ".CMD"; ".VBS"; ".JS"; ".WS"; ".MSC" |]
                Expect.equal "the cmd.exe built-in list is used when PATHEXT is unset" cmdBuiltInList PathEnvironment.pathExt
            )

        testCase "pathExt is a single empty extension when PATHEXT is unset (non-Windows only)" <| fun () ->
            if isWindows then skiptest "PATHEXT is used on Windows"

            withEnvVar "PATHEXT" null (fun () ->
                Expect.equal "non-Windows systems don't use PATHEXT" [| "" |] PathEnvironment.pathExt
            )

        testCase "path drops empty entries produced by a leading, trailing or doubled separator" <| fun () ->
            let sep = string Path.PathSeparator
            let dir1 = Path.Combine(Path.GetTempPath(), "d1_" + Guid.NewGuid().ToString("N"))
            let dir2 = Path.Combine(Path.GetTempPath(), "d2_" + Guid.NewGuid().ToString("N"))
            let pathValue = sep + dir1 + sep + sep + dir2 + sep

            withEnvVar "PATH" pathValue (fun () ->
                Expect.equal "empty PATH entries are dropped" [| dir1; dir2 |] PathEnvironment.path
            )

        testCase "pathExt drops empty entries produced by a leading, trailing or doubled separator (Windows only)" <| fun () ->
            if not isWindows then skiptest "PATHEXT is only used on Windows"

            let sep = string Path.PathSeparator
            let pathExtValue = sep + ".COM" + sep + sep + ".EXE" + sep

            withEnvVar "PATHEXT" pathExtValue (fun () ->
                Expect.equal "empty PATHEXT entries are dropped" [| ".COM"; ".EXE" |] PathEnvironment.pathExt
            )

        testCase "findExecutable finds a name that already has an executable extension" <| fun () ->
            withTmpDir (fun pathDir ->
                let baseName = "marker_" + Guid.NewGuid().ToString("N")
                let exePath = Path.Combine(pathDir.FullName, baseName + ".exe")
                let exePathUpperExt = Path.Combine(pathDir.FullName, baseName + ".EXE")
                let exePathWeirdCaseExt = Path.Combine(pathDir.FullName, baseName + ".ExE")
                touch exePath

                withEnvVar "PATH" pathDir.FullName (fun () ->
                    withEnvVar "PATHEXT" ".COM;.EXE;.BAT" (fun () ->
                        Expect.equal "with the extension (same case)" (Some exePath) (PathEnvironment.findExecutable (baseName + ".exe") false)
                        if Environment.OSVersion.Platform = PlatformID.Win32NT then
                            Expect.equal "with the extension (other case)" (Some exePathWeirdCaseExt) (PathEnvironment.findExecutable (baseName + ".ExE") false)
                            Expect.equal "without the extension" (Some exePathUpperExt) (PathEnvironment.findExecutable baseName false)
                    )
                )
            )

        testCase "findExecutable doesn't consider a file with a non-executable extension (Windows only)" <| fun () ->
            if not isWindows then skiptest "PATHEXT is only used on Windows"

            withTmpDir (fun pathDir ->
                let baseName = "marker_" + Guid.NewGuid().ToString("N")
                touch (Path.Combine(pathDir.FullName, baseName + ".txt"))
                touch (Path.Combine(pathDir.FullName, baseName))

                withEnvVar "PATH" pathDir.FullName (fun () ->
                    withEnvVar "PATHEXT" ".COM;.EXE;.BAT" (fun () ->
                        Expect.equal "other extension" None (PathEnvironment.findExecutable (baseName + ".txt") false)
                        Expect.equal "no extension" None (PathEnvironment.findExecutable baseName false)
                    )
                )
            )
    ]
