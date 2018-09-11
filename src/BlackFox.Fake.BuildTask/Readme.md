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
