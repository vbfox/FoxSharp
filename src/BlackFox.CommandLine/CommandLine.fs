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