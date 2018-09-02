module BlackFox.MasterOfFoo.Build.Tasks

open Fake.Api
open Fake.BuildServer
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators
open Fake.Tools

open BlackFox
open BlackFox.TypedTaskDefinitionHelper
open System.Xml.Linq

type ProjectToBuild = {
    Name: string
    BinDir: string
    ProjectFile: string
    NupkgDir: string
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
    let getReleaseNotes (projectName: string) =
        let fromFile = ReleaseNotes.load (srcDir </> projectName </> "Release Notes.md")
        if BuildServer.buildServer <> BuildServer.LocalBuild then
            let buildVersion = int BuildServer.buildVersion
            let nugetVer = sprintf "%s-appveyor%04i" fromFile.NugetVersion buildVersion
            let asmVer = System.Version.Parse(fromFile.AssemblyVersion)
            let asmVer =
                System.Version(
                    versionPartOrZero asmVer.Major,
                    versionPartOrZero asmVer.Minor,
                    versionPartOrZero asmVer.Build,
                    versionPartOrZero buildVersion)
            ReleaseNotes.ReleaseNotes.New(asmVer.ToString(), nugetVer, fromFile.Date, fromFile.Notes)
        else
            fromFile

    let createProjectInfo name =
        {
            Name = name
            BinDir = artifactsDir </> name </> (string configuration)
            ProjectFile = srcDir </> name </> (name + ".fsproj")
            NupkgDir = artifactsDir </> name </> (string configuration)
            ReleaseNotes = getReleaseNotes name
        }

    let projects =
        [
            createProjectInfo "BlackFox.Fake.BuildTask"
        ]

    /// GitHub info
    let gitOwner = "vbfox"
    let gitHome = "https://github.com/" + gitOwner
    let gitName = "FoxSharp"

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

    let init = task "Init" [] {
        Directory.create artifactsDir
    }

    let clean = task "Clean" [init] {
        let objDirs = projects |> List.map(fun p -> System.IO.Path.GetDirectoryName(p.ProjectFile) </> "obj")
        Shell.cleanDirs (artifactsDir :: objDirs)
    }

    let generateVersionInfo = task "GenerateVersionInfo" [init; clean.IfNeeded] {
        for p in projects do
            writeVersionProps p
    }

    let build = task "Build" [generateVersionInfo; clean.IfNeeded] {
        for p in projects do
            DotNet.build
                (fun o -> { o with Configuration = configuration })
                p.ProjectFile

        DotNet.build
            (fun o -> { o with Configuration = configuration })
            testProjectFile
    }

    let runTests = task "Test" [build] {
        let baseTestDir = artifactsDir </> testProjectName </> (string configuration)
        [
            baseTestDir </> "net461" </> (testProjectName + ".exe")
            baseTestDir </> "netcoreapp2.0" </> (testProjectName + ".dll")
        ]
            |> ExpectoDotNetCli.run (fun p ->
                { p with
                    PrintVersion = false
                    FailOnFocusedTests = true
                })
    }

    let nuget = task "NuGet" [build] {
        for p in projects do
            let projectRelease = getReleaseNotes p.Name

            DotNet.pack
                (fun o -> { o with Configuration = configuration })
                p.ProjectFile
            let nupkgFile =
                p.NupkgDir </> (sprintf "%s.%s.nupkg" p.Name projectRelease.NugetVersion)

            Trace.publish ImportData.BuildArtifact nupkgFile
    }

    let publishNuget = task "PublishNuget" [nuget] {
        let key =
            match Environment.environVarOrNone "nuget-key" with
            | Some(key) -> key
            | None -> UserInput.getUserPassword "NuGet key: "

        for p in projects do
            Paket.push <| fun o ->  { o with WorkingDir = p.NupkgDir; ApiKey = key }
    }

    let zip = task "Zip" [build] {
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

    let _releaseTask = EmptyTask "Release" [clean; publishNuget]
    let _ciTask = EmptyTask "CI" [clean; runTests; zip; nuget]

    EmptyTask "Default" [runTests]
