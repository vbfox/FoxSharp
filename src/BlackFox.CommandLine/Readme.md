# Command line

[![Nuget Package](https://img.shields.io/nuget/v/BlackFox.CommandLine.svg)](https://www.nuget.org/packages/BlackFox.CommandLine)

Generate, parse and escape command lines.

## API

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
