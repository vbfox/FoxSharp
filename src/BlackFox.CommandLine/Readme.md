# Command line

![Command prompt Logo](https://raw.githubusercontent.com/vbfox/FoxSharp/master/src/BlackFox.CommandLine/Icon.png)

[![Nuget Package](https://img.shields.io/nuget/v/BlackFox.CommandLine.svg)](https://www.nuget.org/packages/BlackFox.CommandLine)

Generate, parse and escape command lines.

## Security

The escaping done by this library targets the code that turns a command line **string** back into an argument
**array**: the Microsoft C Runtime / `CommandLineToArgvW` on Windows, the equivalent parser in
`System.Diagnostics.Process` on .Net Core and .Net 5+, and Mono's own parser on Unix. `CmdLine.toString` detects the
runtime it's on and picks the matching one, so its result is safe to assign to `ProcessStartInfo.Arguments`.

It is **not** shell escaping, and passing the result to a shell can let an argument run commands of its own:

* Never set `ProcessStartInfo.UseShellExecute` to `true` with it, and never pass it to `cmd.exe /c`, `sh -c`,
  `bash -c` or `powershell -Command`. Shell metacharacters (`&`, `|`, `>`, `` ` ``, `$`, `;`, `^`, …) are not
  quoted by any of the escaping rules implemented here, because the parsers they target give them no special
  meaning. A shell does.
* On Windows, starting a `.bat` or `.cmd` file always goes through `cmd.exe`, even from `Process.Start` with
  `UseShellExecute = false`. Arguments escaped for the C runtime are therefore not safe for a batch file — this is
  the [BatBadBut][BatBadBut] class of vulnerability ([CVE-2024-24576][CVE-2024-24576] and friends). Invoke the
  interpreter or the real executable directly, or validate the arguments against an allow-list.
* `MonoUnixCommandLine.escape` escapes `'` as `\'` inside a single quoted block because that is what Mono's parser
  expects. A POSIX shell does not: it would end the quoted block there and run the rest of the argument as code.
* `CmdLine.appendRaw` is inserted verbatim, quoting is the caller's job. Never build a raw argument out of a value
  you don't control.

If your target accepts an argument array (`ProcessStartInfo.ArgumentList` on .Net Core 2.1+, `execve`, …) prefer it
over any escaped string, there is then nothing to escape and nothing to get wrong.

`MsvcrCommandLine.parse` implements the rules that apply to the argument string. The name of the executable at the
start of a full Windows command line follows different rules (backslashes are never escape characters there), so
don't use `parse` to decide which program a command line would start.

[BatBadBut]: https://flatt.tech/research/posts/batbadbut-you-cant-securely-execute-commands-on-windows/
[CVE-2024-24576]: https://github.com/advisories/GHSA-q455-m56c-85mh

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

The `CmdLine` record and module implement a simple, pipeable API to generate command lines.

#### empty `CmdLine`

Create an empty command line.

```fsharp
CmdLine.empty
|> CmdLine.toString // (empty string)
```

#### concat `CmdLine seq -> CmdLine`

Concatenate command lines

```fsharp
let first = CmdLine.empty |> CmdLine.append "foo"
let second = CmdLine.empty |> CmdLine.append "--bar"

CmdLine.concat [first; second]
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

#### EscapeSettings

Record type of settings for `escape`:

* `AlwaysQuoteArguments: bool`: Specify that arguments should always be quoted, even simple values
* `DoubleQuoteEscapeQuote`: Use `""` to escape a quote, otherwise `\"` is used
    * Forces all arguments containing quotes to be surrounded by quotes
    * This isn't compatible with pre-2008 msvcr

#### escape `EscapeSettings -> seq<string> -> string`

Escape arguments in a form that programs parsing it as Microsoft C Runtime will successfully understand.

#### parse `string -> string list`

Parse a string representing arguments as the Microsoft C Runtime does.

[MsvcrtParsing]: http://www.daviddeley.com/autohotkey/parameters/parameters.htm#WINARGV

## Thanks

* [Newaita icon pack](https://github.com/cbrnix/Newaita) for the base of the icon (License: [CC BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/))
* [@matthid](https://github.com/matthid) for [finding a bug](https://github.com/vbfox/FoxSharp/issues/1) when comparing this implementation to the one in FAKE 5
