namespace BlackFox.CommandLine

/// Escape arguments in a form that programs parsing it as Microsoft C Runtime will successfuly understand
/// Rules taken from http://www.daviddeley.com/autohotkey/parameters/parameters.htm#WINARGV
module MsvcrCommandLine =
    open System.Text

    let private addBackslashes (builder:StringBuilder) (backslashes: int) (beforeQuote: bool) =
        if backslashes <> 0 then
            // Always using 'backslashes * 2' would work it would just produce needless '\'
            let count =
                if beforeQuote || backslashes % 2 = 0 then
                    backslashes * 2
                else
                    (backslashes-1) * 2 + 1

            for _ in [0..count-1] do
                builder.Append('\\') |> ignore

    let private escapeArgCore (arg : string) (builder : StringBuilder) needQuote =
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
                    builder.Append('\\') |> ignore
                    builder.Append(c) |> ignore
                    escape (pos+1) 0
                | _ ->
                    addBackslashes builder backslashes false
                    builder.Append(c) |> ignore
                    escape (pos+1) 0

        escape 0 0

    let escapeArg (arg : string) (builder : StringBuilder) =
        builder.EnsureCapacity(arg.Length + builder.Length) |> ignore

        if arg.Length = 0 then
            // Empty arg
            builder.Append(@"""""") |> ignore
        else
            let mutable needQuote = false
            let mutable containsQuoteOrBackslash = false

            for c in arg do
                needQuote <- needQuote || (c = ' ')
                needQuote <- needQuote || (c = '\t')
                containsQuoteOrBackslash <- containsQuoteOrBackslash || (c = '"')
                containsQuoteOrBackslash <- containsQuoteOrBackslash || (c = '\\')

            if (not containsQuoteOrBackslash) && (not needQuote) then
                // No special characters are present, early exit
                builder.Append(arg) |> ignore
            else
                // Complex case, we really need to escape
                if needQuote then builder.Append('"') |> ignore
                escapeArgCore arg builder needQuote
                if needQuote then builder.Append('"') |> ignore

    let escape cmdLine =
        let builder = StringBuilder()
        cmdLine |> Seq.iteri (fun i arg ->
            if (i <> 0) then builder.Append(' ') |> ignore
            escapeArg arg builder)

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

type CmdLineArgType = | Normal of string | Raw of string

type CmdLine = {
    Args: CmdLineArgType list
    }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module CmdLine =
    open System
    open System.Text

    let empty = { Args = List.empty }

    let appendRaw value (cmdLine : CmdLine) =
        { cmdLine with Args = Raw(value) :: cmdLine.Args }

    let appendRawIfSome value (cmdLine : CmdLine) =
        match value with
        | Some(value) -> appendRaw value cmdLine
        | None -> cmdLine

    let concat (other : CmdLine) (cmdLine : CmdLine) =
        { cmdLine with Args = other.Args @ cmdLine.Args }

    let append (value : 'a) (cmdLine : CmdLine) =
        let s =
            match box value with
            | :? String as sv -> sv
            | _ -> sprintf "%A" value

        { cmdLine with Args = Normal(s) :: cmdLine.Args }

    let appendSeq (values: 'a seq) (cmdLine : CmdLine) =
        values |> Seq.fold (fun state o -> append o state) cmdLine

    let appendIfTrue cond value cmdLine =
        if cond then cmdLine |> append value else cmdLine

    let appendIfSome value cmdLine =
        match value with
        | Some(value) -> cmdLine |> append value
        | None -> cmdLine

    let appendSeqIfSome values (cmdLine : CmdLine) =
        match values with
        | Some(value) -> appendSeq value cmdLine
        | None -> cmdLine

    let appendIfNotNullOrEmpty value prefix cmdLine =
        appendIfTrue (not (String.IsNullOrEmpty(value))) (prefix+value) cmdLine

    let inline private argsInOrder cmdLine =
        cmdLine.Args |> List.rev

    let private escape escapeFun cmdLine =
        let builder = StringBuilder()
        cmdLine |> argsInOrder |> Seq.iteri (fun i arg ->
            if (i <> 0) then builder.Append(' ') |> ignore
            match arg with
            | Normal(arg) -> escapeFun arg builder
            | Raw(arg) -> builder.Append(arg) |> ignore)

        builder.ToString()

    let toStringForMsvcr cmdLine =
        escape (MsvcrCommandLine.escapeArg) cmdLine

    let toString cmdLine =
        toStringForMsvcr cmdLine

    let fromSeq (values : string seq) =
        values |> Seq.fold (fun state o -> append o state) empty