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

let mkTmpDir () =
    Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "BlackFox.PathEnvironment.Tests_" + Guid.NewGuid().ToString("N")))

[<Tests>]
let tests =
    testSequenced <| testList "PathEnvironment" [
        testCase "findFile with includeCurrentDirectory=false doesn't search the CWD because of a trailing PATH separator" <| fun () ->
            let root = mkTmpDir ()
            try
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
            finally
                Directory.Delete(root.FullName, true)

        testCase "findFile still finds a directory listed on PATH when there is also a trailing separator" <| fun () ->
            let root = mkTmpDir ()
            try
                let onPath = Directory.CreateDirectory(Path.Combine(root.FullName, "on-path"))
                let markerName = "marker_" + Guid.NewGuid().ToString("N") + ".txt"
                let markerPath = Path.Combine(onPath.FullName, markerName)
                File.WriteAllText(markerPath, "")

                let pathWithTrailingSeparator = onPath.FullName + string Path.PathSeparator

                withEnvVar "PATH" pathWithTrailingSeparator (fun () ->
                    let found = PathEnvironment.findFile markerName false
                    Expect.equal "The file on PATH must still be found" (Some markerPath) found
                )
            finally
                Directory.Delete(root.FullName, true)

        testCase "path doesn't throw and is empty when PATH is unset" <| fun () ->
            withEnvVar "PATH" null (fun () ->
                Expect.equal "" [||] PathEnvironment.path
            )

        testCase "pathExt doesn't throw when PATHEXT is unset" <| fun () ->
            withEnvVar "PATHEXT" null (fun () ->
                PathEnvironment.pathExt |> ignore
            )

        testCase "path drops empty entries produced by a leading, trailing or doubled separator" <| fun () ->
            let sep = string Path.PathSeparator
            let dir1 = Path.Combine(Path.GetTempPath(), "d1_" + Guid.NewGuid().ToString("N"))
            let dir2 = Path.Combine(Path.GetTempPath(), "d2_" + Guid.NewGuid().ToString("N"))
            let pathValue = sep + dir1 + sep + sep + dir2 + sep

            withEnvVar "PATH" pathValue (fun () ->
                Expect.equal "empty PATH entries are dropped" [| dir1; dir2 |] PathEnvironment.path
            )

        testCase "findExecutable finds a name that already has an executable extension" <| fun () ->
            let root = mkTmpDir ()
            try
                let onPath = Directory.CreateDirectory(Path.Combine(root.FullName, "on-path"))
                let baseName = "marker_" + Guid.NewGuid().ToString("N")
                let exePath = Path.Combine(onPath.FullName, baseName + ".exe")
                File.WriteAllText(exePath, "")

                withEnvVar "PATH" onPath.FullName (fun () ->
                    withEnvVar "PATHEXT" ".COM;.EXE;.BAT" (fun () ->
                        Expect.equal "with the extension (same case)" (Some exePath) (PathEnvironment.findExecutable (baseName + ".exe") false)
                        if Environment.OSVersion.Platform = PlatformID.Win32NT then
                            Expect.equal "with the extension (other case)" (Some exePath) (PathEnvironment.findExecutable (baseName + ".EXE") false)
                            Expect.equal "without the extension" (Some exePath) (PathEnvironment.findExecutable baseName false)
                    )
                )
            finally
                Directory.Delete(root.FullName, true)

        testCase "findExecutable doesn't consider a file with a non-executable extension (Windows only)" <| fun () ->
            if Environment.OSVersion.Platform = PlatformID.Win32NT then
                let root = mkTmpDir ()
                try
                    let onPath = Directory.CreateDirectory(Path.Combine(root.FullName, "on-path"))
                    let baseName = "marker_" + Guid.NewGuid().ToString("N")
                    File.WriteAllText(Path.Combine(onPath.FullName, baseName + ".txt"), "")
                    File.WriteAllText(Path.Combine(onPath.FullName, baseName), "")

                    withEnvVar "PATH" onPath.FullName (fun () ->
                        withEnvVar "PATHEXT" ".COM;.EXE;.BAT" (fun () ->
                            Expect.equal "other extension" None (PathEnvironment.findExecutable (baseName + ".txt") false)
                            Expect.equal "no extension" None (PathEnvironment.findExecutable baseName false)
                        )
                    )
                finally
                    Directory.Delete(root.FullName, true)

        testCase "pathExt drops empty entries produced by a leading, trailing or doubled separator (Windows only)" <| fun () ->
            if Environment.OSVersion.Platform = PlatformID.Win32NT then
                let sep = string Path.PathSeparator
                let pathExtValue = sep + ".COM" + sep + sep + ".EXE" + sep

                withEnvVar "PATHEXT" pathExtValue (fun () ->
                    Expect.equal "empty PATHEXT entries are dropped" [| ".COM"; ".EXE" |] PathEnvironment.pathExt
                )
    ]
