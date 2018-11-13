/// Parse and escape arguments in a form that programs parsing it as Microsoft C Runtime will successfuly understand
module BlackFox.CommandLine.MsvcrCommandLine

// The best explanation for the rule and especially their history can be found in
// http://www.daviddeley.com/autohotkey/parameters/parameters.htm#WINARGV
// The current version of the behavior is also documented by microsoft at
// https://docs.microsoft.com/en-us/windows/desktop/api/shellapi/nf-shellapi-commandlinetoargvw

open System.Text

/// Settings for the escape method
type EscapeSettings = {
    /// Specify that arguments should always be quoted, even simple values
    AlwaysQuoteArguments: bool

    /// Use `""` to escape a quote, otherwise `\"` is used
    /// Notes:
    ///
    /// * Forces all arguments containing quotes to be surrounded by quotes
    /// * This isn't compatible with pre-2008 msvcrt
    DoubleQuoteEscapeQuote: bool }

/// Default escape settings
let defaultEscapeSettings = {
    AlwaysQuoteArguments = false
    DoubleQuoteEscapeQuote = false }

let private addBackslashes (builder: StringBuilder) (backslashes: int) (beforeQuote: bool) =
    if backslashes <> 0 then
        // Always using 'backslashes * 2' would work it would just produce needless '\'
        let count =
            if not beforeQuote then
                // backslashes not followed immediately by a double quotation mark are interpreted literally
                backslashes
            else
                backslashes * 2

        for _ in [0..count-1] do
            builder.Append('\\') |> ignore

let private escapeArgCore (settings: EscapeSettings) (arg : string) (builder : StringBuilder) needQuote =
    let rec escape pos backslashes =
        if pos >= arg.Length then
            addBackslashes builder backslashes needQuote
            ()
        else
            let c = arg.[pos]
            match c with
            | '\\' ->
                escape (pos+1) (backslashes+1)
            | '"' ->
                addBackslashes builder backslashes true
                if settings.DoubleQuoteEscapeQuote then
                    builder.Append("\"\"") |> ignore
                else
                    builder.Append("\\\"") |> ignore
                escape (pos+1) 0
            | _ ->
                addBackslashes builder backslashes false
                builder.Append(c) |> ignore
                escape (pos+1) 0

    escape 0 0

let internal escapeArg (settings: EscapeSettings) (arg : string) (builder : StringBuilder) =
    builder.EnsureCapacity(arg.Length + builder.Length) |> ignore

    if arg.Length = 0 then
        // Empty arg
        builder.Append(@"""""") |> ignore
    else
        let mutable needQuote = settings.AlwaysQuoteArguments
        let mutable containsQuoteOrBackslash = false

        if not settings.AlwaysQuoteArguments then
            for c in arg do
                needQuote <- needQuote || (c = ' ')
                needQuote <- needQuote || (c = '\t')
                if settings.DoubleQuoteEscapeQuote then
                    // Double quote escaping can only work inside quoted blocks
                    // (Also it's specific to post-2008 msvcrt)
                    needQuote <- needQuote || (c = '"')
                containsQuoteOrBackslash <- containsQuoteOrBackslash || (c = '"')
                containsQuoteOrBackslash <- containsQuoteOrBackslash || (c = '\\')

        if (not containsQuoteOrBackslash) && (not needQuote) then
            // No special characters are present, early exit
            builder.Append(arg) |> ignore
        else
            // Complex case, we really need to escape
            if needQuote then builder.Append('"') |> ignore
            escapeArgCore settings arg builder needQuote
            if needQuote then builder.Append('"') |> ignore

let escape (settings: EscapeSettings) cmdLine =
    let builder = StringBuilder()
    cmdLine |> Seq.iteri (fun i arg ->
        if (i <> 0) then builder.Append(' ') |> ignore
        escapeArg settings arg builder)

    builder.ToString()

let private parseBackslashes (backslashes: Ref<int>) (buffer: StringBuilder) (c: char) =
    if c = '\\' then
        backslashes := !backslashes + 1
        true
    else if !backslashes <= 0 then
        false
    else if c = '"' then
        if !backslashes % 2 = 0 then
            // Even number of backslashes, the backslashes are considered escaped but not the quote
            buffer.Append('\\', !backslashes/2) |> ignore
            backslashes := 0
            false
        else
            // Odd number of backslashes, the backslashes are considered escaped and the quote too
            buffer.Append('\\', (!backslashes-1)/2) |> ignore
            buffer.Append(c) |> ignore
            backslashes := 0
            true
    else
        // Backslashes not followed by a quote are interpreted literally
        buffer.Append('\\', !backslashes) |> ignore
        backslashes := 0
        false

let parse (args: string) =
    if isNull args || args.Length = 0 then
        []
    else
        let buffer = StringBuilder(args.Length)
        let backslashes = ref 0
        let rec f pos result inParameter quoted =
            if pos >= args.Length then
                if inParameter then
                    buffer.Append('\\', !backslashes) |> ignore
                    let parsedArg = buffer.ToString()
                    parsedArg :: result
                else
                    result
            else
                let c = args.[pos]

                let inParameter = inParameter || ((c <> ' ') && (c <> '\t'))
                if not inParameter then
                    // Whitespace between parameters
                    f (pos+1) result false false
                else if parseBackslashes backslashes buffer c then
                    // Some '\' where handled
                    f (pos+1) result inParameter quoted
                else if quoted then
                    if c = '"' then
                        if pos + 1 < args.Length && args.[pos + 1] = '"' then
                            // Special double quote case, insert only one quote and continue the double quoted part
                            // "post 2008" behavior in http://www.daviddeley.com/autohotkey/parameters/parameters.htm#WINARGV
                            buffer.Append(c) |> ignore
                            f (pos+2) result true true
                        else
                            // End of the quoted part
                            f (pos+1) result true false
                    else
                        // Normal character in quoted part
                        buffer.Append(c) |> ignore
                        f (pos+1) result true true
                else
                    match c with
                    | ' '
                    | '\t' ->
                        // End of parameter
                        let parsedArg = buffer.ToString()
                        buffer.Clear() |> ignore
                        f (pos+1) (parsedArg::result) false false
                    | '"' ->
                        // All escapes have been handled so it start a quoted part
                        f (pos+1) result true true
                    | _ ->
                        // Normal character
                        buffer.Append(c) |> ignore
                        f (pos+1) result true false

        f 0 [] false false |> List.rev
