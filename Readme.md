🦊 Sharp
========

A collection of F# modules.

Each module/file is made to be used with [Paket][Paket]'s [GitHub dependencies][GhDeps] feature.

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
open BlackFox.PathEnvironment

match find "node" with
| None -> failwith "nodejs wasn't found"
| nodePath -> // ...
```

Paket: `github vbfox/FoxSharp src/BlackFox.FoxSharp/PathEnvironment.fs`

Command line
------------

Generate, parse and escape command lines.

The `MsvcrCommandLine` module is specific to the way the [Microsoft C Runtime algorithm][MsvcrtParsing] works on Windows. It's how the vast majority of arguments are parsed on the Windows platform.

* `escape: seq<string> -> string`: Escape arguments in a form that programs parsing it as Microsoft C Runtime will successfuly understand.
* `parse: string -> string list`: Parse a string representing arguments as the Microsoft C Runtime does.

The `CmdLine` record and module implement a simple, pipable API to generate command lines.

* `empty: CmdLine`: Represents an empty command line
* `appendRaw: CmdLine -> string -> CmdLine`: Add a part of the command line that won't be escaped
* `appendRawIfSome: CmdLine -> string option -> CmdLine`
* `concat: CmdLine -> CmdLine -> CmdLine`: Concat 2 command lines
* `append: CmdLine -> string -> CmdLine`: Append an argument to the command line
* `appendSeq: CmdLine -> seq<string> -> CmdLine`: Append arguments to the command line
* `appendIfTrue: CmdLine -> bool -> string -> CmdLine`: Append arguments to the command line if the condition is true
* `appendIfSome: CmdLine -> string option -> CmdLine`
* `appendSeqIfSome: CmdLine -> string list option -> CmdLine`
* `appendIfNotNullOrEmpty: CmdLine -> string -> string -> CmdLine`
* `toStringForMsvcr: CmdLine -> string`
* `toString: CmdLine -> string`: Transform the command line into a string, escaping as needed by the current OS/Framework for `Process.Start`.
* `fromSeq: seq<string> -> CmdLine`: Create a command line from a sequence of arguments

[MsvcrtParsing]: http://www.daviddeley.com/autohotkey/parameters/parameters.htm#WINARGV

Paket: `github vbfox/FoxSharp src/BlackFox.FoxSharp/CommandLine.fs`

FAKE Tasks
----------

A replacement for FAKE `Target` using a syntax similar to Gulp

Examples:

```fsharp
open BlackFox.TaskDefinitionHelper

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
open BlackFox.TypedTaskDefinitionHelper

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