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
