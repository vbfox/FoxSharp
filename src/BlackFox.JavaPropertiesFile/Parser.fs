module private BlackFox.JavaPropertiesFile.Parser

open System.Text
open System.IO
open System.Globalization

type CharReader = unit -> char option

let inline (|IsWhitespace|_|) c =
    match c with
    | Some c -> if c = ' ' || c = '\t' || c = '\u00ff' then Some c else None
    | None -> None

type IsEof =
    | Yes = 1y
    | No = 0y

let rec readToFirstChar (c: char option) (reader: CharReader) =
    match c with
    | IsWhitespace _ ->
        readToFirstChar (reader ()) reader
    | Some '\r'
    | Some '\n' ->
        None, IsEof.No
    | Some _ -> c, IsEof.No
    | None -> None, IsEof.Yes

let inline (|EscapeSequence|_|) c =
    match c with
    | Some c ->
        if c = 'r' || c = 'n' || c = 'u' || c = 'f' || c = 't' || c = '"' || c = ''' || c = '\\' then
            Some c
        else
            None
    | None -> None

let inline isHex c = (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')

let readEscapeSequence (c: char) (reader: CharReader) =
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

let inline readKey (c: char option) (reader: CharReader) (buffer: StringBuilder) =
    let rec recurseEnd (result: string) =
        match reader () with
        | Some ':'
        | Some '='
        | IsWhitespace _ -> recurseEnd result
        | Some '\r'
        | Some '\n' -> result, false, None, IsEof.No
        | None -> result, false, None, IsEof.Yes
        | Some c -> result, true, Some c, IsEof.No
    let rec recurse (c: char option) (buffer: StringBuilder) (escaping: bool) =
        match c with
        | EscapeSequence c when escaping ->
            let realChar = readEscapeSequence c reader
            recurse (reader()) (buffer.Append(realChar)) false
        | Some ' ' -> recurseEnd (buffer.ToString())
        | Some ':'
        | Some '=' when not escaping -> recurseEnd (buffer.ToString())
        | Some '\r'
        | Some '\n' -> buffer.ToString(), false, None, IsEof.No
        | None -> buffer.ToString(), false, None, IsEof.Yes
        | Some '\\' -> recurse (reader ()) buffer true
        | Some c -> recurse (reader ()) (buffer.Append(c)) false

    recurse c buffer false

let rec readComment (reader: CharReader) (buffer: StringBuilder) =
    match reader () with
    | Some '\r'
    | Some '\n' ->
        Some (Comment (buffer.ToString())), IsEof.No
    | None ->
        Some(Comment (buffer.ToString())), IsEof.Yes
    | Some c ->
        readComment reader (buffer.Append(c))

let inline readValue (c: char option) (reader: CharReader) (buffer: StringBuilder) =
    let rec recurse (c: char option) (buffer: StringBuilder) (escaping: bool) (cr: bool) (lineStart: bool) =
        match c with
        | EscapeSequence c when escaping ->
            let realChar = readEscapeSequence c reader
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
            let firstChar, _ = readToFirstChar c reader
            recurse firstChar buffer false false false
        | Some '\\' -> recurse (reader ()) buffer true false false
        | Some c ->
            recurse (reader()) (buffer.Append(c)) false false false

    recurse c buffer false false true

let rec readLine (reader: CharReader) (buffer: StringBuilder) =
    match readToFirstChar (reader ()) reader with
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

    [|
        while isEof <> IsEof.Yes do
            let line, isEofAfterLine = readLine reader buffer
            match line with
            | Some line -> yield line
            | None -> ()
            isEof <- isEofAfterLine
    |]
