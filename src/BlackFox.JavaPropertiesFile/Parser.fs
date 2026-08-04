module private BlackFox.JavaPropertiesFile.Parser

open System.Text
open System.IO
open System.Globalization

type CharReader = unit -> char option

let inline (|IsWhitespace|_|) c =
    match c with
    | Some c -> if c = ' ' || c = '\t' || c = '\f' then Some c else None
    | None -> None

type IsEof =
    | Yes = 1y
    | No = 0y

/// Starting from `c` read until a non whitespace is found if any.
/// Stop at end of lines
let rec readToFirstCharOrEolOrEof (c: char option) (reader: CharReader) =
    match c with
    | IsWhitespace _ ->
        readToFirstCharOrEolOrEof (reader ()) reader
    | Some '\r'
    | Some '\n' ->
        None, IsEof.No
    | Some _ -> c, IsEof.No
    | None -> None, IsEof.Yes

// Match a character to know if it's a potential escape sequence second char ('n' returns Some('n')
// because "\n" is a valid escape sequence.
let inline (|EscapeSequence|_|) c =
    match c with
    | Some c ->
        if c = 'r' || c = 'n' || c = 'u' || c = 'f' || c = 't' || c = '"' || c = ''' || c = '\\' then
            Some c
        else
            None
    | None -> None

/// Returns if a character is a valid hexadecimal character (Upper or Lower case)
let inline isHex c = (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')

/// Read the characters following `\` in an escape sequence. Supporting unicode escapes.
///
/// - returns the passed-in character if it's not a known escape sequence
/// - throws if an unicode escape is invalid
let readEscapeSequenceAfterBackSlash (c: char) (reader: CharReader) =
    match c with
    | 'r' -> '\r'
    | 'n' -> '\n'
    | 'f' -> '\f'
    | 't' -> '\t'
    | 'u' ->
        match reader(), reader(), reader(), reader() with
        | Some c1, Some c2, Some c3, Some c4 when isHex c1 && isHex c2 && isHex c3 && isHex c4 ->
            let hex = System.String([|c1;c2;c3;c4|])
            let value = System.UInt16.Parse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)
            char value
        | _ ->
            failwith "Invalid unicode escape"
    | _ -> c

/// Read the key part of a key=value line
let inline readKey (c: char option) (reader: CharReader) (buffer: StringBuilder) =
    /// Skip whitespace and, at most, a single ':' or '=' separator between the key and the value.
    let rec recurseEnd (result: string) (hasSep: bool) =
        match reader () with
        | Some ':'
        | Some '=' when not hasSep -> recurseEnd result true
        | IsWhitespace _ -> recurseEnd result hasSep
        | Some '\r'
        | Some '\n' -> result, false, None, IsEof.No
        | None -> result, false, None, IsEof.Yes
        | Some c -> result, true, Some c, IsEof.No

    let rec recurse (c: char option) (buffer: StringBuilder) (escaping: bool) (cr: bool) (lineStart: bool) =
        match c with
        | EscapeSequence c when escaping ->
            let realChar = readEscapeSequenceAfterBackSlash c reader
            recurse (reader()) (buffer.Append(realChar)) false false false
        | Some '\r'
        | Some '\n' ->
            if escaping || (cr && c = Some '\n') then
                recurse (reader ()) buffer false (c = Some '\r') true
            else
                buffer.ToString(), false, None, IsEof.No
        | None -> buffer.ToString(), false, None, IsEof.Yes
        | Some _ when lineStart ->
            match readToFirstCharOrEolOrEof c reader with
            | None, eof -> buffer.ToString(), false, None, eof
            | Some firstChar, _ -> recurse (Some firstChar) buffer false false false
        | IsWhitespace _ when not escaping -> recurseEnd (buffer.ToString()) false
        | Some ':'
        | Some '=' when not escaping -> recurseEnd (buffer.ToString()) true
        | Some '\\' -> recurse (reader ()) buffer true false false
        | Some c -> recurse (reader ()) (buffer.Append(c)) false false false

    recurse c buffer false false false

let rec readComment (reader: CharReader) (buffer: StringBuilder) =
    match reader () with
    | Some '\r'
    | Some '\n' ->
        Some (Comment (buffer.ToString())), IsEof.No
    | None ->
        Some(Comment (buffer.ToString())), IsEof.Yes
    | Some c ->
        readComment reader (buffer.Append(c))

/// Read the value part of a key=value line
let inline readValue (c: char option) (reader: CharReader) (buffer: StringBuilder) =
    let rec recurse (c: char option) (buffer: StringBuilder) (escaping: bool) (cr: bool) (lineStart: bool) =
        match c with
        | EscapeSequence c when escaping ->
            let realChar = readEscapeSequenceAfterBackSlash c reader
            recurse (reader()) (buffer.Append(realChar)) false false false
        | Some '\r'
        | Some '\n' ->
            if escaping || (cr && c = Some '\n') then
                recurse (reader ()) buffer false (c = Some '\r') true
            else
                buffer.ToString(), IsEof.No
        | None ->
            buffer.ToString(), IsEof.Yes
        | Some _ when lineStart ->
            match readToFirstCharOrEolOrEof c reader with
            | None, eof -> buffer.ToString(), eof
            | Some firstChar, _ -> recurse (Some firstChar) buffer false false false
        | Some '\\' -> recurse (reader ()) buffer true false false
        | Some c ->
            recurse (reader()) (buffer.Append(c)) false false false

    recurse c buffer false false true

let rec readLine (reader: CharReader) (buffer: StringBuilder) =
    match readToFirstCharOrEolOrEof (reader ()) reader with
    | Some '#', _
    | Some '!', _ ->
        readComment reader (buffer.Clear())
    | Some firstChar, _ ->
        let key, hasValue, c, isEof = readKey (Some firstChar) reader (buffer.Clear())
        let value, isEof =
            if hasValue then
                // We know that we aren't at the end of the buffer, but readKey can return None if it didn't need the next char
                let firstChar = match c with | Some c -> Some c | None -> reader ()
                readValue firstChar reader (buffer.Clear())
            else
                "", isEof
        Some (KeyValue(key, value)), isEof
    | None, isEof -> None, isEof

let inline textReaderToReader (reader: TextReader) =
    let buffer = [| '\u0000' |]
    fun () ->
        let eof = reader.Read(buffer, 0, 1) = 0
        if eof then None else Some (buffer.[0])

let parseWithReader reader =
    let buffer = StringBuilder(255)
    let mutable isEof = IsEof.No

    [
        while isEof <> IsEof.Yes do
            let line, isEofAfterLine = readLine reader buffer
            match line with
            | Some line -> yield line
            | None -> ()
            isEof <- isEofAfterLine
    ]
