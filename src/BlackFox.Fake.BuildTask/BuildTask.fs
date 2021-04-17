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
let createFn (name: string) (dependencies: TaskInfo list) (body: TargetParameter -> unit): TaskInfo =
    let metadata = {Name = name; Dependencies = dependencies }
    registerTask metadata body
    infoFromMeta metadata

/// Define a Task with it's dependencies
let create (name: string) (dependencies: TaskInfo list): TaskBuilder =
    let metadata = {Name = name; Dependencies = dependencies }
    TaskBuilder(metadata)

/// Define a Task without any body, only dependencies
let createEmpty (name: string) (dependencies: TaskInfo list): TaskInfo =
    let metadata = {Name = name; Dependencies = dependencies }
    registerTask metadata ignore
    infoFromMeta metadata

/// Run the task specified on the command line if there was one or the
/// default one otherwise.
let runOrDefault (defaultTask: TaskInfo): unit =
    match defaultTask.Metadata with
    | Some metadata -> Target.runOrDefault metadata.Name
    | None -> failwith "No default task specified."

/// Run the task specified on the command line if there was one or the
/// default one otherwise.
let runOrDefaultWithArguments (defaultTask: TaskInfo): unit =
    match defaultTask.Metadata with
    | Some metadata -> Target.runOrDefaultWithArguments metadata.Name
    | None -> failwith "No default task specified."

/// Runs the task given by the target parameter or lists the available targets
let runOrList (): unit =
    Target.runOrList ()

/// List all tasks available.
let listAvailable (): unit =
    Target.listAvailable ()

/// <summary>Writes a dependency graph.</summary>
/// <param name="verbose">Whether to print verbose output or not.</param>
/// <param name="taskInfo">The target for which the dependencies should be printed.</param>
let printDependencyGraph (verbose: bool) (taskInfo: TaskInfo): unit =
    match taskInfo.Metadata with
    | Some metadata ->
        Target.printDependencyGraph verbose metadata.Name
    | None ->
        failwith "No default task specified."

/// Setup the FAKE context from a program argument
///
/// Arguments are the same as the ones comming after "run" when running via FAKE.
/// The only difference is that "--target" is apended if the first argument doesn't start with "-".
///
/// Examples:
///
///  * `foo` -> `run --target foo`
///  * `--target bar --baz` -> `run --target bar --baz`
let setupContextFromArgv (argv: string []): unit =
    let argvTweaked =
        match List.ofArray argv with
        | firstArg :: rest when not (firstArg.StartsWith("-")) ->
            [ "--target"; firstArg ] @ rest
        | argv -> argv
    let execContext = Context.FakeExecutionContext.Create false "build.fsx" argvTweaked
    Context.setExecutionContext (Fake.Core.Context.RuntimeContext.Fake execContext)

let private temporaryColor (color: ConsoleColor) : IDisposable =
    let colorBefore = Console.ForegroundColor
    Console.ForegroundColor <- color
    { new IDisposable with member __.Dispose() = Console.ForegroundColor <- colorBefore }

let private appWrap (app: unit -> unit): int =
    try
        app ()
        0
    with
    | ex ->
        use __ = temporaryColor ConsoleColor.Red
        printfn "%O" ex
        1

/// Run the task specified on the command line if there was one or the
/// default one otherwise. Return 0 on success and 1 on error, printing
/// the exception on the console.
let runOrDefaultApp (defaultTask: TaskInfo): int =
    appWrap (fun _ -> runOrDefault defaultTask)

/// Runs the task given by the target parameter or lists the available targets.
/// Return 0 on success and 1 on error, printing the exception on the console.
let runOrListApp (): int =
    appWrap (fun _ -> runOrList ())
