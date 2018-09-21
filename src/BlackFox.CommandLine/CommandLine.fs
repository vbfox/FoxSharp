namespace BlackFox.CommandLine

type CmdLineArgType =
    | Normal of string
    | Raw of string

type CmdLine = {
    Args: CmdLineArgType list }

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

    let appendf<'T> (format: StringFormat<'T, CmdLine -> CmdLine>): 'T =
        ksprintf append format

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

    let fromSeq (values : string seq) =
        values |> Seq.fold (fun state o -> append o state) empty

    let toStringForMsvcr cmdLine =
        escape (MsvcrCommandLine.escapeArg) cmdLine

    let toString cmdLine =
        toStringForMsvcr cmdLine
