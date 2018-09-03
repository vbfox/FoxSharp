# Path environment

[![Nuget Package](https://img.shields.io/nuget/v/BlackFox.PathEnvironment.svg)](https://www.nuget.org/packages/BlackFox.PathEnvironment)

Parse and expose both PATH and PATHEXT environment variables and allow to find an executable with the same rules as the
shell.

## API

* `path: Lazy<string list>`: Directories in the system PATH.
* `pathExt: Lazy<string list>` Extensions considered executables by the system.
  Parsed from `PATHEXT` on windows and always return `[""]` on other systems.
* `findInPathOnly: string -> string option`: Find an executable in `PATH` (Extension is optional if present in
  `PATHEXT`)
* `find: string -> string option`: Find an executable in the current directory or `PATH` (Extension is optional if
  present in `PATHEXT`)

## Example

```fsharp
open BlackFox.PathEnvironment

match find "node" with
| None -> failwith "nodejs wasn't found"
| nodePath -> // ...
```
