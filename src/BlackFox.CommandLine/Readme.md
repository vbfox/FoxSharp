# Command line

![Command prompt Logo](https://raw.githubusercontent.com/vbfox/FoxSharp/master/src/BlackFox.CommandLine/Icon.png)

[![Nuget Package](https://img.shields.io/nuget/v/BlackFox.CommandLine.svg)](https://www.nuget.org/packages/BlackFox.CommandLine)

Generate, parse and escape command lines.

## API

```fsharp
open BlackFox.CommandLine

type Configuration = | Release | Debug
let noRestore = true
let framework = Some "netstandard2.0"
let configuration = Release

let cmd =
    CmdLine.empty
    |> CmdLine.append "build"
    |> CmdLine.appendIf noRestore "--no-restore"
    |> CmdLine.appendPrefixIfSome "--framework" framework
    |> CmdLine.appendPrefixf "--configuration" "%A" configuration
    |> CmdLine.toString

// dotnet build --no-restore --framework netstandard2.0 --configuration Release
printfn "dotnet %s" cmd
```

### CmdLine

The `CmdLine` record and module implement a simple, pipable API to generate command lines.

#### empty `CmdLine`

Create an empty command line.

```fsharp
CmdLine.empty
|> CmdLine.toString // (empty string)
```

#### concat `CmdLine -> CmdLine -> CmdLine`

Concatenate two command lines (First the second one then the first one)

```fsharp
let other = CmdLine.empty |> CmdLine.append "--bar"
CmdLine.empty
|> CmdLine.append "foo"
|> CmdLine.concat other
|> CmdLine.toString // foo --bar
```

#### appendRaw `string -> CmdLine`

Append a raw (Non escaped) argument to a command line.

```fsharp
CmdLine.empty
|> CmdLine.appendRaw "foo bar"
|> CmdLine.appendRaw "baz"
|> CmdLine.toString // foo bar baz
```

#### append `string -> CmdLine`

Append an argument to a command line.

```fsharp
CmdLine.empty
|> CmdLine.append "foo bar"
|> CmdLine.append ""
|> CmdLine.append "baz"
|> CmdLine.toString // "foo bar" "" baz
```

#### appendf `StringFormat<'T, CmdLine -> CmdLine> -> 'T`

Append an argument to a command line using printf-like syntax.

```fsharp
CmdLine.empty
|> CmdLine.appendf "--values=%s" "foo bar"
|> CmdLine.toString // "--values=foo bar"
```

#### appendPrefix `string -> string -> CmdLine`

Append an argument prefixed by another.

```fsharp
CmdLine.empty
|> CmdLine.appendPrefix "--foo" "bar baz"
|> CmdLine.toString // --foo "bar baz"
```

#### appendPrefixf `string -> StringFormat<'T, CmdLine -> CmdLine> -> 'T`

Append an argument prefixed by another using printf-like syntax.

```fsharp
CmdLine.empty
|> CmdLine.appendPrefixf "--foo" "+:%s" "bar"
|> CmdLine.toString // --foo +:bar
```

#### appendIf `bool -> string -> CmdLine -> CmdLine`

Append an argument to a command line if a condition is true.

```fsharp
CmdLine.empty
|> CmdLine.appendIf true "--foo"
|> CmdLine.appendIf false "--bar"
|> CmdLine.toString // --foo
```

#### appendIff `bool -> StringFormat<'T, CmdLine -> CmdLine> -> 'T`

Append an argument to a command line if a condition is true using printf-like syntax.

```fsharp
CmdLine.empty
|> CmdLine.appendIff true "--foo=%s" "baz"
|> CmdLine.appendIff false "--bar=%s" "baz"
|> CmdLine.toString // --foo=baz
```

#### appendPrefixIf `bool -> string -> string -> CmdLine -> CmdLine`

Append an argument to a command line if a condition is true.

```fsharp
CmdLine.empty
|> CmdLine.appendPrefixIf true "--foo" "baz"
|> CmdLine.appendPrefixIf false "--bar" "baz"
|> CmdLine.toString // --foo baz
```

#### appendPrefixIff `bool -> string -> StringFormat<'T, CmdLine -> CmdLine> -> 'T`

Append an argument to a command line if a condition is true using printf-like syntax.

```fsharp
CmdLine.empty
|> CmdLine.appendPrefixIff "--foo" true "+:%s" "baz"
|> CmdLine.appendPrefixIff "--bar" false "+:%s" "baz"
|> CmdLine.toString // --foo +:baz
```

#### appendIfSome `string option -> CmdLine -> CmdLine`

Append an argument to a command line if the value is Some.

```fsharp
CmdLine.empty
|> CmdLine.appendIfSome (Some "--foo")
|> CmdLine.appendIfSome None
|> CmdLine.toString // --foo
```

#### appendIfSomef `StringFormat<'TArg -> string> -> 'TArg option -> CmdLine -> CmdLine`

Append an argument to a command line if the value is Some using printf-like syntax (With a single argument).

```fsharp
CmdLine.empty
|> CmdLine.appendIfSomef "--foo=%s" (Some "baz")
|> CmdLine.appendIfSomef "--bar=" None
|> CmdLine.toString // --foo=baz
```

#### appendPrefixIfSome `string -> string option -> CmdLine -> CmdLine`

Append an argument prefixed by another if the value is Some.

```fsharp
CmdLine.empty
|> CmdLine.appendPrefixIfSome "--foo" (Some "baz")
|> CmdLine.appendPrefixIfSome "--bar" None
|> CmdLine.toString // --foo baz
```

#### appendPrefixIfSomef `string -> StringFormat<'TArg -> string> -> 'TArg option -> CmdLine -> CmdLine`

Append an argument prefixed by another if the value is Some using printf-like syntax (With a single argument).

```fsharp
CmdLine.empty
|> CmdLine.appendPrefixIfSomef "--foo" "+:%s" (Some "baz")
|> CmdLine.appendPrefixIfSomef "--bar" "+:%s" None
|> CmdLine.toString // --foo +:baz
```

#### appendSeq `string seq -> CmdLine -> CmdLine`

Append a sequence of arguments.

```fsharp
CmdLine.empty
|> CmdLine.appendSeq ["--foo"; "bar"]
|> CmdLine.toString // --foo bar
```

#### appendSeqf `StringFormat<'TArg -> string> -> string seq -> CmdLine -> CmdLine`

Append a sequence of arguments using printf-like syntax (With a single argument).

```fsharp
CmdLine.empty
|> CmdLine.appendSeqf "--foo=%s" ["bar"; "baz"]
|> CmdLine.toString // --foo=bar --foo=baz
```

#### appendPrefixSeq `string -> string seq -> CmdLine -> CmdLine`

Append a sequence of arguments each being prefixed.

```fsharp
CmdLine.empty
|> CmdLine.appendPrefixSeq "--add" ["foo"; "bar"; "baz"]
|> CmdLine.toString // --add foo --add bar --add baz
```

#### appendPrefixSeqf `string -> StringFormat<'TArg -> string> -> string seq -> CmdLine -> CmdLine`

Append a sequence of arguments each being prefixed using printf-like syntax (With a single argument).

```fsharp
CmdLine.empty
|> CmdLine.appendPrefixSeqf "--add" "./%s" ["bar"; "baz"]
|> CmdLine.toString // --add ./bar ./baz
```

#### appendIfNotNullOrEmpty `string -> CmdLine -> CmdLine`

Append an argument if the value isn't null or empty.

```fsharp
CmdLine.empty
|> CmdLine.appendIfNotNullOrEmpty ""
|> CmdLine.appendIfNotNullOrEmpty "--foo"
|> CmdLine.appendIfNotNullOrEmpty null
|> CmdLine.toString // --foo
```

#### appendIfNotNullOrEmptyf `StringFormat<string -> string> -> string -> CmdLine -> CmdLine`

Append an argument if the value isn't null or empty using printf-like syntax (With a single string argument).

```fsharp
CmdLine.empty
|> CmdLine.appendIfNotNullOrEmptyf "--foo=%s" ""
|> CmdLine.appendIfNotNullOrEmptyf "--foo=%s" "bar"
|> CmdLine.appendIfNotNullOrEmptyf "--foo=%s" null
|> CmdLine.toString // --foo=bar
```

#### appendPrefixIfNotNullOrEmpty `string -> string -> CmdLine -> CmdLine`

Append an argument if the value isn't null or empty.

```fsharp
CmdLine.empty
|> CmdLine.appendPrefixIfNotNullOrEmpty "--foo" ""
|> CmdLine.appendPrefixIfNotNullOrEmpty "--foo" "bar"
|> CmdLine.appendPrefixIfNotNullOrEmpty "--foo" null
|> CmdLine.toString // --foo bar
```

#### appendPrefixIfNotNullOrEmptyf `string -> StringFormat<string -> string> -> string -> CmdLine -> CmdLine`

Append an argument prefixed by another if the value isn't null or empty using printf-like syntax (With a single string argument).

```fsharp
CmdLine.empty
|> CmdLine.appendPrefixIfNotNullOrEmptyf "--foo" "./%s" ""
|> CmdLine.appendPrefixIfNotNullOrEmptyf "--foo" "./%s" "bar"
|> CmdLine.appendPrefixIfNotNullOrEmptyf "--foo" "./%s" null
|> CmdLine.toString // --foo ./bar
```

#### fromSeq `string seq -> CmdLine`

Create a command line from a sequence of arguments.

```fsharp
seq { yield "foo bar"; yield "baz" }
|> CmdLine.fromSeq
|> CmdLine.toString // "foo bar" baz
```

#### fromList `string list -> CmdLine`

Create a command line from a list of arguments.

```fsharp
["foo bar"; "baz"]
|> CmdLine.fromList
|> CmdLine.toString // "foo bar" baz
```

#### fromArray `string [] -> CmdLine`

Create a command line from a list of arguments.

```fsharp
[|"foo bar"; "baz"|]
|> CmdLine.fromArray
|> CmdLine.toString // "foo bar" baz
```

#### toList `CmdLine -> string list`

Get a list of arguments from a command line (No escaping is applied).

```fsharp
CmdLine.empty
|> CmdLine.append "foo bar"
|> CmdLine.append "baz"
|> CmdLine.toList // ["foo bar"; "baz"]
```

#### toArray `CmdLine -> string []`

Get an array of arguments from a command line (No escaping is applied).

```fsharp
CmdLine.empty
|> CmdLine.append "foo bar"
|> CmdLine.append "baz"
|> CmdLine.toArray // [|"foo bar"; "baz"|]
```

#### toStringForMsvcr `MsvcrCommandLine.EscapeSettings -> CmdLine -> string`

Convert a command line to string using the [Microsoft C Runtime][MsvcrtParsing] (Windows default) rules.

#### toString `CmdLine -> string`

Convert a command line to string as expected by `System.Diagnostics.Process`.

### MsvcrCommandLine

The `MsvcrCommandLine` module is specific to the way the [Microsoft C Runtime algorithm][MsvcrtParsing] works on Windows. It's how the vast majority of arguments are parsed on the Windows platform.

### EscapeSettings

Record type of settings for `escape`:

* `AlwaysQuoteArguments: bool`: Specify that arguments should always be quoted, even simple values
* `DoubleQuoteEscapeQuote`: Use `""` to escape a quote, otherwise `\"` is used
    * Forces all arguments containing quotes to be surrounded by quotes
    * This isn't compatible with pre-2008 msvcrt

#### escape `EscapeSettings -> seq<string> -> string`

Escape arguments in a form that programs parsing it as Microsoft C Runtime will successfuly understand.

#### parse `string -> string list`

Parse a string representing arguments as the Microsoft C Runtime does.

[MsvcrtParsing]: http://www.daviddeley.com/autohotkey/parameters/parameters.htm#WINARGV

## Thanks

* [Newaita icon pack](https://github.com/cbrnix/Newaita) for the base of the icon (License: [CC BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/))
* [@matthid](https://github.com/matthid) for [finding a bug](https://github.com/vbfox/FoxSharp/issues/1) when comparing this implementation to the one in FAKE 5
