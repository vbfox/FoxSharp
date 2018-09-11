# FAKE BuildTask

![Orange FAKE Logo](https://raw.githubusercontent.com/vbfox/FoxSharp/master/src/BlackFox.Fake.BuildTask/Icon.png)

A strongly typed `Target` alternative for [FAKE](https://fake.build/)

[![Nuget Package](https://img.shields.io/nuget/v/BlackFox.Fake.BuildTask.svg)](https://www.nuget.org/packages/BlackFox.Fake.BuildTask)

## Examples

```fsharp
open BlackFox.Fake

// A task with no dependencies
let paketRestore = BuildTask.create "PaketRestore" [] {
    // ...
}

// A task that need the restore to be done and should run after Clean
// if it is in the build chain
let build = BuildTask.create "Build" [clean.IfNeeded; paketRestore] {
    // ...
}

// A task without any action, only dependencies here specifying what should
// run in CI
let _ci = BuildTask.createEmpty "CI" [clean; build]

RunTaskOrDefault build
```

## API

```fsharp
module BlackFox.Fake.BuildTask
    /// What FAKE name a target
    type TaskMetadata = {
        Name: string
        Dependencies: TaskInfo list
    }

    /// Dependency (Soft or Hard) to a target (That can be a null object)
    type TaskInfo = {
        Metadata: TaskMetadata option
        IsSoft: bool
    }
    with
        static member NoTask
        member this.Always with get()
        member this.IfNeeded with get()
        member this.If(condition: bool)

    /// Define a Task with it's dependencies
    let createFn (name: string) (dependencies: TaskInfo list) (body: TargetParameter -> unit): TaskInfo

    /// Define a Task with it's dependencies
    let create (name: string) (dependencies: TaskInfo list): TaskBuilder

    /// Define a Task without any body, only dependencies
    let createEmpty (name: string) (dependencies: TaskInfo list): TaskInfo

    /// Run the task specified on the command line if there was one or the
    /// default one otherwise.
    let runOrDefault (defaultTask: TaskInfo): unit

    /// Run the task specified on the command line if there was one or the
    /// default one otherwise.
    let runOrDefaultWithArguments (defaultTask: TaskInfo): unit

    /// Runs the task given by the target parameter or lists the available targets
    let runOrList (): unit

    /// List all tasks available.
    let listAvailable (): unit

    /// Writes a dependency graph.
    let printDependencyGraph (verbose: bool) (taskInfo: TaskInfo): unit

    /// Setup the FAKE context from a program argument
    ///
    /// Arguments are the same as the ones comming after "run" when running via FAKE.
    /// The only difference is that "--target" is apended if the first argument doesn't start with "-".
    ///
    /// Examples:
    ///
    ///  * `foo` -> `run --target foo`
    ///  * `--target bar --baz` -> `run --target bar --baz`
    let setupContextFromArgv (argv: string []): unit

    /// Run the task specified on the command line if there was one or the
    /// default one otherwise. Return 0 on success and 1 on error, printing
    /// the exception on the console.
    let runOrDefaultApp (defaultTask: TaskInfo): int

    /// Runs the task given by the target parameter or lists the available targets.
    /// Return 0 on success and 1 on error, printing the exception on the console.
    let runOrListApp (): int
```
