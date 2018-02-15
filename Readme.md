🦊 Sharp
========

A collection of F# modules.

Each module/file is made to be used with [Paket][Paket] [GitHub dependencies][GhDeps] feature.

[Paket]: https://fsprojects.github.io
[GhDeps]: https://fsprojects.github.io/Paket/github-dependencies.html

Path environment
----------------

Parse and expose both PATH and PATHEXT environment variables and allow to find an executable with the same rules as the
shell.

* `path: Lazy<string list>`: Directories in the system PATH.
* `pathExt: Lazy<string list>` Extensions considered executables by the system.
  Parsed from `PATHEXT` on windows and always return `[""]` on other systems.
* `findInPathOnly: string -> string option`: Find an executable in `PATH` (Extension is optional if present in
  `PATHEXT`)
* `find: string -> string option`: Find an executable in the current directory or `PATH` (Extension is optional if
  present in `PATHEXT`)

Example:

```fsharp
match find "node" with
| None -> failwith "nodejs wasn't found"
| nodePath -> // ...
```

Paket: `github vbfox/FoxSharp src/BlackFox.FoxSharp/PathEnvironment.fs`

Command line
------------

Generate, parse and escape command lines.

Paket: `github vbfox/FoxSharp src/BlackFox.FoxSharp/CommandLine.fs`

FAKE Tasks
----------

A replacement for FAKE `Target` using a syntax similar to Gulp

Examples:

```fsharp
// A task with no dependencies
Task "Clean" [] (fun _ ->
    // ...
)

// Another one, defined with the computation expression
task "PaketRestore" [] {
    // ...
}

// A task that need the restore to be done and should run after Clean
// if it is in the build chain
task "Build" ["?Clean";"PaketRestore"] {
    // ...
}

EmptyTask "CI" ["Clean";"PaketRestore"]

RunTaskOrDefault "Build"
```

Paket: `github vbfox/FoxSharp src/BlackFox.FakeUtils/TaskDefinitionHelper.fs`

FAKE Typed tasks
----------------

A replacement for FAKE `Target` using a syntax similar to Gulp with no string in
dependency definitions.

Examples:

```fsharp
// A task with no dependencies
let clean = Task "Clean" [] (fun _ ->
    // ...
)

// Another one, defined with the computation expression
let paketRestore = task "PaketRestore" [] {
    // ...
}

// A task that need the restore to be done and should run after Clean
// if it is in the build chain
let build = task "Build" [clean.IfNeeded;paketRestore] {
    // ...
}

let _ci = EmptyTask "CI" [clean;build]

RunTaskOrDefault build
```

Paket: `github vbfox/FoxSharp src/BlackFox.FakeUtils/TypedTaskDefinitionHelper.fs`