/// Allow to define FAKE tasks with a syntax similar to Gulp tasks
[<RequireQualifiedAccess>]
module BlackFox.Fake.BuildTask

open Fake.Core
open Fake.Core.TargetOperators
open System

/// What FAKE name a target
type TaskMetadata = {
    Name: string
    Dependencies: TaskInfo list
}
/// Dependency (Soft or Hard) to a target (That can be a null object)
and TaskInfo = {
    Metadata: TaskMetadata option
    IsSoft: bool
}
with
    static member NoTask =
        { Metadata = None; IsSoft = false }

    member this.Always
        with get() = { this with IsSoft = false }

    member this.IfNeeded
        with get() = { this with IsSoft = true }

    member this.If(condition: bool) =
        if condition then this else TaskInfo.NoTask

/// Register dependencies of the passed Task in FAKE
let inline private applyTaskDependecies meta =
    for dependency in meta.Dependencies do
        match dependency.Metadata with
        | Some dependencyMetadata ->
            if dependency.IsSoft then
                dependencyMetadata.Name ?=> meta.Name |> ignore
            else
                dependencyMetadata.Name ==> meta.Name |> ignore
        | None -> ()

/// Register the Task for FAKE with all it's dependencies
let inline private registerTask meta body =
    Target.create meta.Name body
    applyTaskDependecies meta

let inline private infoFromMeta meta =
    { Metadata = Some meta; IsSoft = false }

type TaskBuilder(metadata: TaskMetadata) =
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
        registerTask metadata f
        infoFromMeta metadata

/// Define a Task with it's dependencies
let createFn name dependencies body =
    let metadata = {Name = name; Dependencies = dependencies }
    registerTask metadata body
    infoFromMeta metadata

/// Define a Task with it's dependencies
let create (name: string) (dependencies: TaskInfo list) =
    let metadata = {Name = name; Dependencies = dependencies }
    TaskBuilder(metadata)

/// Define a Task without any body, only dependencies
let createEmpty (name: string) (dependencies: TaskInfo list) =
    let metadata = {Name = name; Dependencies = dependencies }
    registerTask metadata ignore
    infoFromMeta metadata

/// Run the task specified on the command line if there was one or the
/// default one otherwise.
let runOrDefault (defaultTask: TaskInfo) =
    match defaultTask.Metadata with
    | Some metadata -> Target.runOrDefault metadata.Name
    | None -> failwith "No default task specified."

/// Run the task specified on the command line if there was one or the
/// default one otherwise.
let runOrDefaultWithArguments (defaultTask: TaskInfo) =
    match defaultTask.Metadata with
    | Some metadata -> Target.runOrDefaultWithArguments metadata.Name
    | None -> failwith "No default task specified."

/// Runs the task given by the target parameter or lists the available targets
let runOrList () =
    Target.runOrList ()

/// List all tasks available.
let listAvailable () =
    Target.listAvailable ()

let printDependencyGraph (verbose: bool) (taskInfo: TaskInfo) =
    match taskInfo.Metadata with
    | Some metadata ->
        Target.printDependencyGraph verbose metadata.Name
    | None ->
        failwith "No default task specified."
