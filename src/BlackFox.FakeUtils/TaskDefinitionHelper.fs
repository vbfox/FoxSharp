/// Allow to define FAKE tasks with a syntax similar to Gulp tasks
/// From https://gist.github.com/vbfox/e3e22d9ffff9b9de7f51
module BlackFox.TaskDefinitionHelper

open Fake
open System
open System.Text.RegularExpressions

type Dependency =
    | Direct of dependsOn : string
    | Soft of dependsOn : string

let inline private (|RegExp|_|) pattern input =
    let m = Regex.Match(input, pattern, RegexOptions.Compiled)
    if m.Success && m.Groups.Count > 0 then
        Some (m.Groups.[1].Value)
    else
        None

let inline private parseDependency str =
    match str with
    | RegExp @"^\?(.*)$" dep -> Soft dep
    | dep -> Direct dep

let inline private parseDependencies dependencies =
    [
        for dependency in dependencies do
            yield parseDependency dependency
    ]

type TaskMetadata = {
    name: string
    dependencies: Dependency list
}

let mutable private tasks : TaskMetadata list = []

let inline private registerTask meta body =
    Target meta.name body
    tasks <- meta::tasks

type TaskBuilder(meta: TaskMetadata) =
    member __.TryFinally(f, compensation) =
        try
            f()
        finally
            compensation()
    member __.TryWith(f, catchHandler) =
        try
            f()
        with e -> catchHandler e
    member __.Using(disposable: #IDisposable, f) =
        try
            f disposable
        finally
            match disposable with
            | null -> ()
            | disp -> disp.Dispose()
    member __.For(sequence, f) =
        for i in sequence do f i
    member __.Combine(f1, f2) = f2(); f1
    member __.Zero() = ()
    member __.Delay f = f
    member __.Run f =
        registerTask meta (fun () -> f())

/// Define a task with it's dependencies
let Task name dependencies body =
    registerTask { name = name; dependencies = parseDependencies dependencies } body

/// Define a task with it's dependencies
let task name dependencies =
    TaskBuilder({ name = name; dependencies = parseDependencies dependencies })

/// Define a task without any body, only dependencies
let EmptyTask name dependencies =
    registerTask { name = name; dependencies = parseDependencies dependencies } (fun () -> ())

/// Send all the defined inter task dependencies to FAKE
let ApplyTasksDependencies () =
        for taskMetadata in tasks do
        for dependency in taskMetadata.dependencies do
            match dependency with
            | Direct dep -> dep ==> taskMetadata.name |> ignore
            | Soft dep -> dep ?=> taskMetadata.name |> ignore

        tasks <- []

/// Run the task specified on the command line if there was one or the
/// default one otherwise.
let RunTaskOrDefault taskName =
    ApplyTasksDependencies ()
    RunTargetOrDefault taskName