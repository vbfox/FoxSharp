module BlackFox.ExpectoDotNetCli

open System
open Fake.Core
open Fake.DotNet.Testing
open Fake.Testing.Common

/// Execution mode for an expecto test assembly
type ExpectoRunMode =
    /// Auto-detect the execution mode from the assembly file extension
    | Auto
    /// Directly run the test assembly executable
    | Direct
    /// Run the test assembly via 'dotnet'
    | DotNet

type private ResolvedRunMode = | ResolvedDirect | ResolvedDotNet

let private getRunMode (configured: ExpectoRunMode) (assembly: string) =
    match configured with
    | Auto ->
        let ext = System.IO.Path.GetExtension(assembly).ToLowerInvariant()
        match ext with
        | ".dll" -> ResolvedDotNet
        | ".exe" -> ResolvedDirect
        | _ ->
            failwithf "Unable to find a way to run expecto test executable with extension %s" ext
    | Direct -> ResolvedDirect
    | DotNet -> ResolvedDotNet

let run (setParams : Expecto.Params -> Expecto.Params) (assemblies : string seq) =
    let details = assemblies |> String.separated ", "
    use __ = Trace.traceTask "Expecto" details

    let runAssembly testAssembly =
        let exitCode =
            let fakeStartInfo testAssembly (args: Expecto.Params) =
                let runMode = getRunMode Auto testAssembly
                let workingDir =
                    if String.isNotNullOrEmpty args.WorkingDirectory
                    then args.WorkingDirectory else Fake.IO.Path.getDirectory testAssembly
                let fileName, argsString =
                    match runMode with
                    | ResolvedDotNet ->
                        "dotnet", sprintf "\"%s\" %O" testAssembly args
                    | ResolvedDirect ->
                        testAssembly, string args
                (fun (info: ProcStartInfo) ->
                    { info with
                        FileName = fileName
                        Arguments = argsString
                        WorkingDirectory = workingDir } )

            let execWithExitCode testAssembly argsString timeout =
                Process.execSimple (fakeStartInfo testAssembly argsString) timeout

            let p = setParams Expecto.Params.DefaultParams
            execWithExitCode testAssembly p TimeSpan.MaxValue

        testAssembly, exitCode

    let res =
        assemblies
        |> Seq.map runAssembly
        |> Seq.filter( snd >> (<>) 0)
        |> Seq.toList

    match res with
    | [] -> ()
    | failedAssemblies ->
        failedAssemblies
        |> List.map (fun (testAssembly,exitCode) ->
            sprintf "Expecto test of assembly '%s' failed. Process finished with exit code %d." testAssembly exitCode )
        |> String.concat System.Environment.NewLine
        |> FailedTestsException |> raise
    __.MarkSuccess()
