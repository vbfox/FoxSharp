/// Parse and escape arguments as Mono does it on Unix platforms in the System.Process type
module BlackFox.CommandLine.MonoUnixCommandLine

open System.Text

/// Settings for the escape method
type EscapeSettings = {
    /// Specify that arguments should always be quoted, even simple values
    AlwaysQuoteArguments: bool
}

/// Default escape settings
let defaultEscapeSettings = {
    AlwaysQuoteArguments = false }

let private escapeArgCore (arg : string) (builder : StringBuilder) =
    for c in arg do
        if c = ''' || c = '\\' then
            builder.Append('\\') |> ignore
        builder.Append(c) |> ignore

let escapeArgumentToBuilder (settings: EscapeSettings) (arg : string) (builder : StringBuilder): unit =
    builder.EnsureCapacity(arg.Length + builder.Length) |> ignore

    if arg.Length = 0 then
        // Empty arg
        builder.Append(@"''") |> ignore
    else
        let mutable needQuote = settings.AlwaysQuoteArguments

        if not settings.AlwaysQuoteArguments then
            for c in arg do
                needQuote <- needQuote || (c = ' ')
                needQuote <- needQuote || (c = '\t')
                needQuote <- needQuote || (c = '\n')
                needQuote <- needQuote || (c = '\v')
                needQuote <- needQuote || (c = '\f')
                needQuote <- needQuote || (c = '\r')
                needQuote <- needQuote || (c = '"')
                needQuote <- needQuote || (c = ''')
                needQuote <- needQuote || (c = '\\')

        if not needQuote then
            // No special characters are present, early exit
            builder.Append(arg) |> ignore
        else
            // Complex case, we really need to escape
            builder.Append(''') |> ignore
            escapeArgCore arg builder
            builder.Append(''') |> ignore

let escapeArgument (settings: EscapeSettings) (arg : string): string =
    let builder = StringBuilder()
    escapeArgumentToBuilder settings arg builder
    builder.ToString()

let escapeToBuilder (settings: EscapeSettings) (cmdLine: seq<string>) (builder: StringBuilder): unit =
    cmdLine |> Seq.iteri (fun i arg ->
        if (i <> 0) then builder.Append(' ') |> ignore
        escapeArgumentToBuilder settings arg builder)

let escape (settings: EscapeSettings) (cmdLine: seq<string>): string =
    let builder = StringBuilder()
    escapeToBuilder settings cmdLine builder
    builder.ToString()

let inline private isAsciiSpace (c: char) =
    c = ' ' || c = '\t' || c = '\n' || c = '\v' || c = '\f' || c = '\r'

let parse (args: string) =
    let mutable escaped = false
    let mutable fresh = true
    let mutable quoteChar = '\x00'
    let str = StringBuilder()
    let mutable result = List.empty
    for i in [0 .. (args.Length-1)] do
        let c = args.[i]
        if escaped then
            if quoteChar = '"' then
                if not (c = '$' || c = '`' || c = '"' || c = '\\') then
                    str.Append('\\') |> ignore
                str.Append(c) |> ignore
            else
                if not (isAsciiSpace c) then
                    str.Append(c) |> ignore
            escaped <- false
        else if quoteChar <> '\x00' then
            if c = quoteChar then
                quoteChar <- '\x00'
                let nextIsSpace = (i <> args.Length - 1) && (isAsciiSpace args.[i + 1])
                if fresh && (nextIsSpace || i = args.Length - 1) then
                    result <- str.ToString() :: result
                    str.Clear() |> ignore
            else if c = '\\' then
                escaped <- true
            else
                str.Append(c) |> ignore
        else if isAsciiSpace c then
            if str.Length > 0 then
                result <- str.ToString() :: result
                str.Clear() |> ignore
        else if c = '\\' then
            escaped <- true
        else if (c = '\'' || c = '"') then
            fresh <- str.Length = 0
            quoteChar <- c
        else
            str.Append(c) |> ignore

    if escaped then
        failwith "Unfinished escape."
    else if quoteChar <> '\x00' then
        failwith "Unfinished quote."
    else
        if str.Length <> 0 then
            result <- str.ToString() :: result

        result |> List.rev
