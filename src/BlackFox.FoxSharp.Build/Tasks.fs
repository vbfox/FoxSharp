module BlackFox.MasterOfFoo.Build.Tasks

open Fake.Core
open Fake.DotNet
open Fake.DotNet.Testing
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators

open BlackFox.Fake
open System.Xml.Linq

type ProjectToBuild = {
    Name: string
    BinDir: string
    ProjectFile: string
    NupkgFile: string
    ReleaseNotes: ReleaseNotes.ReleaseNotes
}

let rootDir = System.IO.Path.GetFullPath(__SOURCE_DIRECTORY__ </> ".." </> "..")
let srcDir = rootDir </> "src"
let artifactsDir = rootDir </> "artifacts"
let testProjectName = "BlackFox.FoxSharp.Tests"
let testProjectFile = srcDir </> testProjectName </> (testProjectName + ".fsproj")

let createAndGetDefault () =
    let configuration = DotNet.BuildConfiguration.fromEnvironVarOrDefault "configuration" DotNet.BuildConfiguration.Release

    let inline versionPartOrZero x = if x < 0 then 0 else x

    let getUnionCaseName (x:'a) =
        match Microsoft.FSharp.Reflection.FSharpValue.GetUnionFields(x, typeof<'a>) with | case, _ -> case.Name

    let getReleaseNotes (projectName: string) =
        let fromFile = ReleaseNotes.load (srcDir </> projectName </> "Release Notes.md")
        if BuildServer.buildServer <> BuildServer.LocalBuild then
            let buildServerName = (getUnionCaseName BuildServer.buildServer).ToLowerInvariant()
            let nugetVer = sprintf "%s-%s.%s" fromFile.NugetVersion buildServerName BuildServer.buildVersion
            ReleaseNotes.ReleaseNotes.New(fromFile.AssemblyVersion, nugetVer, fromFile.Date, fromFile.Notes)
        else
            fromFile

    let createProjectInfo name =
        let bindir = artifactsDir </> name </> (string configuration)
        let releaseNotes = getReleaseNotes name
        let nupkgFile = bindir </> (sprintf "%s.%s.nupkg" name releaseNotes.NugetVersion)
        {
            Name = name
            BinDir = bindir
            ProjectFile = srcDir </> name </> (name + ".fsproj")
            NupkgFile = nupkgFile
            ReleaseNotes = releaseNotes
        }

    let projects =
        [
            createProjectInfo "BlackFox.Fake.BuildTask"
            createProjectInfo "BlackFox.VsWhere"
            createProjectInfo "BlackFox.JavaPropertiesFile"
            createProjectInfo "BlackFox.CommandLine"
            createProjectInfo "BlackFox.PathEnvironment"
            createProjectInfo "BlackFox.CachedFSharpReflection"
        ]

    let writeVersionProps (p: ProjectToBuild) =
        let projectRelease = getReleaseNotes p.Name
        let doc =
            XDocument(
                XElement(XName.Get("Project"),
                    XElement(XName.Get("PropertyGroup"),
                        XElement(XName.Get "Version", projectRelease.NugetVersion),
                        XElement(XName.Get "PackageReleaseNotes", String.toLines projectRelease.Notes))))
        let path = artifactsDir </> p.Name </> "Version.props"

        Directory.create (Path.getDirectory path)
        System.IO.File.WriteAllText(path, doc.ToString())

    let init = BuildTask.create "Init" [] {
        Directory.create artifactsDir
    }

    let clean = BuildTask.create "Clean" [init] {
        let objDirs = projects |> List.map(fun p -> System.IO.Path.GetDirectoryName(p.ProjectFile) </> "obj")
        Shell.cleanDirs (artifactsDir :: objDirs)
    }

    let generateVersionInfo = BuildTask.create "GenerateVersionInfo" [init; clean.IfNeeded] {
        for p in projects do
            writeVersionProps p
    }

    let buildLibraries = BuildTask.create "BuildLibraries" [generateVersionInfo; clean.IfNeeded] {
        for p in projects do
            DotNet.build
                (fun o -> { o with Configuration = configuration })
                p.ProjectFile
    }

    let buildTests = BuildTask.create "BuildTests" [generateVersionInfo; clean.IfNeeded] {
        DotNet.build
            (fun o -> { o with Configuration = configuration })
            testProjectFile
    }

    let build = BuildTask.createEmpty "Build" [buildLibraries; buildTests]

    let runTests = BuildTask.create "Test" [buildTests] {
        let baseTestDir = artifactsDir </> testProjectName </> (string configuration)
        let testConfs = ["netcoreapp2.0", ".dll"]
        let testConfs =
            if Environment.isWindows then
                ("net461", ".exe") :: testConfs
            else
                testConfs

        testConfs
        |> List.map (fun (fw, ext) -> baseTestDir </> fw </> (testProjectName + ext))
        |> Expecto.run (fun p ->
            { p with
                PrintVersion = false
                FailOnFocusedTests = true
            })

        for (fw, _) in testConfs do
            let dir = baseTestDir </> fw
            let outFile = sprintf "TestResults_%s.xml" (fw.Replace('.', '_'))
            File.delete (dir </> outFile)
            (dir </> "TestResults.xml") |> Shell.rename (dir </> outFile)
            Trace.publish (ImportData.Nunit NunitDataVersion.Nunit) (dir </> outFile)
    }

    let nuget = BuildTask.create "NuGet" [build] {
        for p in projects do
            DotNet.pack
                (fun o -> { o with Configuration = configuration })
                p.ProjectFile

            Trace.publish ImportData.BuildArtifact p.NupkgFile
    }

    let publishNuget = BuildTask.create "PublishNuget" [nuget] {
        let key =
            match Environment.environVarOrNone "nuget-key" with
            | Some(key) -> key
            | None -> UserInput.getUserPassword "NuGet key: "

        Paket.pushFiles
            (fun o ->  { o with ApiKey = key })
            (projects |> List.map (fun p -> p.NupkgFile))
    }

    let zip = BuildTask.create "Zip" [build] {
        for p in projects do
            let zipFile = artifactsDir </> (sprintf "%s-%s.zip" p.Name p.ReleaseNotes.NugetVersion)
            let comment = sprintf "%s v%s" p.Name p.ReleaseNotes.NugetVersion
            GlobbingPattern.createFrom p.BinDir
                ++ "**/*.dll"
                ++ "**/*.xml"
                -- "**/FSharp.Core.*"
                |> Zip.createZip p.BinDir zipFile comment 9 false

            Trace.publish ImportData.BuildArtifact zipFile
    }

    let _releaseTask = BuildTask.createEmpty "Release" [clean; publishNuget]
    let _ciTask = BuildTask.createEmpty "CI" [clean; build; runTests; zip; nuget]

    BuildTask.createEmpty "Default" [build; runTests]
