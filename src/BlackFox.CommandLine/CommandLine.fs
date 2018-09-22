namespace BlackFox.CommandLine

open Printf
open System.Text

/// A command line argument
type CmdLineArgType =
    /// Normal command line argument that should be escaped
    | Normal of string
    /// Raw command line argument that won't receive any escaping
    | Raw of string

    override this.ToString() =
        match this with
        | Normal s -> s
        | Raw s -> s

/// A command line
type CmdLine = {
    Args: CmdLineArgType list
} with
    /// An empty command line
    static member Empty: CmdLine =
        { Args = List.empty }

    /// Concatenate another command line
    member this.Concat (other : CmdLine): CmdLine =
        { this with Args = other.Args @ this.Args }

    /// Append a raw (Non escaped) argument
    member this.AppendRaw (value: string): CmdLine =
        { this with Args = Raw(value) :: this.Args }

    //-----------------------------------------------------------------------
    // Direct

    /// Append an argument
    member this.Append (value: string): CmdLine =
        { this with Args = Normal(value) :: this.Args }

    /// Append 2 arguments
    member private this.Append2 (value1: string) (value2: string): CmdLine =
        { this with Args = Normal(value2) :: Normal(value1) :: this.Args }

    /// Append an argument using printf-like syntax
    member this.Appendf<'T> (format: StringFormat<'T, CmdLine>): 'T =
        ksprintf (fun (value: string) -> this.Append value) format

    /// Append an argument prefixed by another
    member this.AppendPrefix (prefix: string) (value: string): CmdLine =
        this.Append2 prefix value

    /// Append an argument prefixed by another using printf-like syntax
    member this.AppendPrefixf<'T> (prefix: string) (format: StringFormat<'T, CmdLine>): 'T =
        ksprintf (fun (value: string) -> this.Append2 prefix value) format

    //-----------------------------------------------------------------------
    // If

    /// Append an argument if a condition is true
    member this.AppendIf (cond: bool) (value : string): CmdLine =
        match cond with
        | true -> this.Append value
        | false -> this

    /// Append an argument if a condition is true using printf-like syntax
    member this.AppendIff<'T> (cond: bool) (format: StringFormat<'T, CmdLine>): 'T =
        ksprintf (fun (value: string) -> this.AppendIf cond value) format

    /// Append an argument prefixed by another if a condition is true
    member this.AppendPrefixIf (cond: bool) (prefix: string) (value : string): CmdLine =
        match cond with
        | true -> this.Append2 prefix value
        | false -> this

    /// Append an argument prefixed by another if a condition is true using printf-like syntax
    member this.AppendPrefixIff<'T> (cond: bool) (prefix: string) (format: StringFormat<'T, CmdLine>): 'T =
        ksprintf (fun (value: string) -> this.AppendPrefixIf cond prefix value) format

    //-----------------------------------------------------------------------
    // Option

    /// Append an argument if the value is Some
    member this.AppendIfSome (value : string option): CmdLine =
        match value with
        | Some value -> this.Append value
        | None -> this

    /// Append an argument if the value is Some using printf-like syntax (With a single argument)
    member this.AppendIfSomef<'TArg> (format: StringFormat<'TArg -> string>) (value: 'TArg option): CmdLine =
        match value with
        | Some value ->
            let s = sprintf format value
            this.Append s
        | None -> this

    /// Append an argument prefixed by another if the value is Some
    member this.AppendPrefixIfSome (prefix: string) (value : string option): CmdLine =
        match value with
        | Some value -> this.Append2 prefix value
        | None -> this

    /// Append an argument prefixed by another if the value is Some using printf-like syntax (With a single argument)
    member this.AppendPrefixIfSomef<'TArg> (prefix: string) (format: StringFormat<'TArg -> string>)
        (value: 'TArg option): CmdLine =
        match value with
        | Some value ->
            let s = sprintf format value
            this.Append2 prefix s
        | None -> this

    //-----------------------------------------------------------------------
    // Sequence

    /// Append a sequence of arguments
    member this.AppendSeq (values: string seq): CmdLine =
        Seq.fold (fun (state: CmdLine) value -> state.Append value) this values

    /// Append a sequence of arguments using printf-like syntax (With a single argument)
    member this.AppendSeqf<'TArg> (format: StringFormat<'TArg -> string>) (values: 'TArg seq): CmdLine =
        Seq.fold (fun (state: CmdLine) value -> state.Append (sprintf format value)) this values

    /// Append a sequence of arguments each being prefixed
    member this.AppendPrefixSeq (prefix: string) (values: string seq): CmdLine =
        Seq.fold (fun (state: CmdLine) value -> state.AppendPrefix prefix value) this values

    /// Append a sequence of arguments each being prefixed using printf-like syntax (With a single argument)
    member this.AppendPrefixSeqf<'TArg> (prefix: string) (format: StringFormat<'TArg -> string>) (values: 'TArg seq)
        : CmdLine =
        Seq.fold (fun (state: CmdLine) value -> state.AppendPrefix prefix (sprintf format value)) this values

    //-----------------------------------------------------------------------
    // Not null or empty

    /// Append an argument if the value isn't null or empty
    member this.AppendIfNotNullOrEmpty (value: string): CmdLine =
        if System.String.IsNullOrEmpty value then
            this
        else
            this.Append value

    /// Append an argument if the value isn't null or empty using printf-like syntax (With a single string argument)
    member this.AppendIfNotNullOrEmptyf (format: StringFormat<string -> string>) (value: string): CmdLine =
        if System.String.IsNullOrEmpty value then
            this
        else
            this.Append (sprintf format value)

    /// Append an argument prefixed by another if the value isn't null or empty
    member this.AppendPrefixIfNotNullOrEmpty (prefix: string) (value: string): CmdLine =
        if System.String.IsNullOrEmpty value then
            this
        else
            this.AppendPrefix prefix value

    /// Append an argument prefixed by another if the value isn't null or empty using printf-like syntax
    /// (With a single string argument)
    member this.AppendPrefixIfNotNullOrEmptyf (prefix: string) (format: StringFormat<string -> string>) (value: string)
        : CmdLine =
        if System.String.IsNullOrEmpty value then
            this
        else
            this.AppendPrefix prefix (sprintf format value)

    //-----------------------------------------------------------------------
    // From

    static member FromList (values : string list): CmdLine =
        { Args = values |> List.map Normal |> List.rev }

    static member FromSeq (values : string seq): CmdLine =
        values |> Seq.toList |> CmdLine.FromList

    static member FromArray (values : string []): CmdLine =
        values |> Array.toList |> CmdLine.FromList

    //-----------------------------------------------------------------------
    // To

    member this.ToList (): string list =
        let mutable result = []
        for arg in this.Args do
            result <- (arg.ToString()) :: result
        result

    member this.ToArray (): string [] =
        let mutable result = Array.zeroCreate this.Args.Length
        let mutable i = this.Args.Length - 1
        for arg in this.Args do
            result.[i] <- arg.ToString()
            i <- i - 1
        result

    member private this.Escape escapeFun =
        let builder = StringBuilder()
        this.Args |> List.rev |> Seq.iteri (fun i arg ->
            if (i <> 0) then builder.Append(' ') |> ignore
            match arg with
            | Normal arg -> escapeFun arg builder
            | Raw arg -> builder.Append(arg) |> ignore)

        builder.ToString()

    member this.ToStringForMsvcr (): string =
        this.Escape (MsvcrCommandLine.escapeArg)

    override this.ToString (): string =
        this.Escape (MsvcrCommandLine.escapeArg)

/// Handle command line arguments
module CmdLine =
    /// An empty command line
    [<CompiledName("Empty")>]
    let empty: CmdLine =
        CmdLine.Empty

    /// Concatenate two command lines (First the second one then the first one)
    [<CompiledName("Concat")>]
    let inline concat (other : CmdLine) (cmdLine : CmdLine): CmdLine =
        cmdLine.Concat other

    /// Append a raw (Non escaped) argument to a command line
    [<CompiledName("AppendRaw")>]
    let inline appendRaw value (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendRaw value

    //-----------------------------------------------------------------------
    // Direct

    /// Append an argument to a command line
    [<CompiledName("Append")>]
    let inline append (value : string) (cmdLine : CmdLine): CmdLine =
        cmdLine.Append value

    /// Append an argument to a command line using printf-like syntax
    [<CompiledName("AppendFormatToCmdLineThen")>]
    let appendf<'T> (format: StringFormat<'T, CmdLine -> CmdLine>): 'T =
        ksprintf append format

    /// Append an argument prefixed by another
    [<CompiledName("AppendPrefix")>]
    let appendPrefix (prefix: string) (value: string) (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendPrefix prefix value

    /// Append an argument prefixed by another using printf-like syntax
    [<CompiledName("AppendPrefixFormatToCmdLineThen")>]
    let appendPrefixf<'T> (prefix: string) (format: StringFormat<'T, CmdLine -> CmdLine>): 'T =
        ksprintf (appendPrefix prefix) format

    //-----------------------------------------------------------------------
    // If

    /// Append an argument to a command line if a condition is true
    [<CompiledName("AppendIf")>]
    let inline appendIf (cond: bool) (value : string) (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendIf cond value

    /// Append an argument to a command line if a condition is true using printf-like syntax
    [<CompiledName("AppendIfFormatToCmdLineThen")>]
    let appendIff<'T> (cond: bool) (format: StringFormat<'T, CmdLine -> CmdLine>): 'T =
        ksprintf (appendIf cond) format

    /// Append an argument prefixed by another if a condition is true
    [<CompiledName("AppendPrefixIf")>]
    let inline appendPrefixIf (cond: bool) (prefix: string) (value : string) (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendPrefixIf cond prefix value

    /// Append an argument prefixed by another if a condition is true using printf-like syntax
    [<CompiledName("AppendPrefixIfFormatToCmdLineThen")>]
    let appendPrefixIff<'T> (cond: bool) (prefix: string) (format: StringFormat<'T, CmdLine -> CmdLine>): 'T =
        ksprintf (appendPrefixIf cond prefix) format

    //-----------------------------------------------------------------------
    // Option

    /// Append an argument to a command line if the value is Some
    [<CompiledName("AppendIfSome")>]
    let inline appendIfSome (value : string option) (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendIfSome value

    /// Append an argument to a command line if the value is Some using printf-like syntax (With a single argument)
    [<CompiledName("AppendIfSomeFormatToCmdLineThen")>]
    let inline appendIfSomef<'TArg> (format: StringFormat<'TArg -> string>) (value: 'TArg option) (cmdLine: CmdLine)
        : CmdLine =
        cmdLine.AppendIfSomef format value

    /// Append an argument prefixed by another if the value is Some
    [<CompiledName("AppendPrefixIfSome")>]
    let inline appendPrefixIfSome (prefix: string) (value : string option) (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendPrefixIfSome prefix value

    /// Append an argument prefixed by another if the value is Some using printf-like syntax (With a single argument)
    [<CompiledName("AppendPrefixIfSomeFormatToCmdLineThen")>]
    let inline appendPrefixIfSomef<'TArg> (prefix: string) (format: StringFormat<'TArg -> string>) (value: 'TArg option)
        (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendPrefixIfSomef prefix format value

    //-----------------------------------------------------------------------
    // Sequence

    /// Append a sequence of arguments
    [<CompiledName("AppendSeq")>]
    let inline appendSeq (values: string seq) (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendSeq values

    /// Append a sequence of arguments using printf-like syntax (With a single argument)
    [<CompiledName("AppendSeqFormatToCmdLineThen")>]
    let inline appendSeqf<'TArg> (format: StringFormat<'TArg -> string>) (values: 'TArg seq) (cmdLine : CmdLine)
        : CmdLine =
        cmdLine.AppendSeqf format values

    /// Append a sequence of arguments each being prefixed
    [<CompiledName("AppendPrefixSeq")>]
    let inline appendPrefixSeq (prefix: string) (values: string seq) (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendPrefixSeq prefix values

    /// Append a sequence of arguments each being prefixed using printf-like syntax (With a single argument)
    [<CompiledName("AppendPrefixSeqFormatToCmdLineThen")>]
    let inline appendPrefixSeqf<'TArg> (prefix: string) (format: StringFormat<'TArg -> string>) (values: 'TArg seq)
        (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendPrefixSeqf prefix format values

    //-----------------------------------------------------------------------
    // Not null or empty

    /// Append an argument if the value isn't null or empty
    [<CompiledName("AppendIfNotNullOrEmpty")>]
    let inline appendIfNotNullOrEmpty (value: string) (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendIfNotNullOrEmpty value

    /// Append an argument if the value isn't null or empty using printf-like syntax (With a single string argument)
    [<CompiledName("AppendIfNotNullOrEmptyFormatToCmdLineThen")>]
    let inline appendIfNotNullOrEmptyf (format: StringFormat<string -> string>) (value: string) (cmdLine : CmdLine)
        : CmdLine =
        cmdLine.AppendIfNotNullOrEmptyf format value

    /// Append an argument prefixed by another if the value isn't null or empty
    [<CompiledName("AppendPrefixIfNotNullOrEmpty")>]
    let inline appendPrefixIfNotNullOrEmpty (prefix: string) (value: string) (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendPrefixIfNotNullOrEmpty prefix value

    /// Append an argument prefixed by another if the value isn't null or empty using printf-like syntax
    /// (With a single string argument)
    [<CompiledName("AppendPrefixIfNotNullOrEmptyFormatToCmdLineThen")>]
    let inline appendPrefixIfNotNullOrEmptyf (prefix: string) (format: StringFormat<string -> string>) (value: string)
        (cmdLine : CmdLine): CmdLine =
        cmdLine.AppendPrefixIfNotNullOrEmptyf prefix format value

    //-----------------------------------------------------------------------
    // From

    [<CompiledName("FromSeq")>]
    let fromSeq (values : string seq) =
        CmdLine.FromSeq values

    [<CompiledName("FromList")>]
    let fromList (values : string list) =
        CmdLine.FromList values

    [<CompiledName("FromArray")>]
    let fromArray (values : string []) =
        CmdLine.FromArray values

    //-----------------------------------------------------------------------
    // To

    [<CompiledName("ToList")>]
    let toList (cmdLine : CmdLine): string list =
        cmdLine.ToList()

    [<CompiledName("ToArray")>]
    let toArray (cmdLine : CmdLine): string [] =
        cmdLine.ToArray()

    [<CompiledName("ToStringForMsvcr")>]
    let toStringForMsvcr (cmdLine : CmdLine): string =
        cmdLine.ToStringForMsvcr()

    [<CompiledName("ToString")>]
    let toString (cmdLine : CmdLine): string =
        cmdLine.ToString()
