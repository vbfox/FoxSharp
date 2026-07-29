# Path environment

[![Nuget Package](https://img.shields.io/nuget/v/BlackFox.PathEnvironment.svg)](https://www.nuget.org/packages/BlackFox.PathEnvironment)

Parse and expose both PATH and PATHEXT environment variables and allow to find an executable with the same rules as the
shell.

## API

```fsharp
type BlackFox.PathEnvironment =
    // Directories in the system PATH.
    path: string []

    // Extensions considered executables by the system.
    // Parsed from `PATHEXT` on windows and always return `[""]` on other systems.
    pathExt: string []

    // Find an executable on the PATH
    findExecutable: name: string -> includeCurrentDirectory: bool -> string option

    // Find a file on the PATH
    findFile: name: string -> includeCurrentDirectory: bool -> string option
```

## Example

```fsharp
open BlackFox

match PathEnvironment.findExecutable "node" false with
| None -> failwith "nodejs wasn't found"
| nodePath -> // ...
```

## Notes

* `includeCurrentDirectory` inserts the current directory **before** the PATH, the way Windows does when it starts a
  process. A file dropped in the current directory then wins over the one installed on the system, so only pass
  `true` when that is what you want.
* Empty `PATH` and `PATHEXT` entries (a leading, trailing or doubled separator, which is common on Windows) are
  ignored. An empty `PATH` entry would otherwise be combined into a relative path and resolved against the current
  directory, searching it even when `includeCurrentDirectory` is `false`.
* `name` is combined with each directory as a relative path, so a name containing directory separators or `..` can
  designate a file outside of the PATH directories. Validate names that come from an untrusted source.
