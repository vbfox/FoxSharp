module BlackFox.JavaPropertiesFile.JavaPropertiesFile

open System.IO

/// <summary>
/// Parse a Java '.properties' file from a <see cref="System.IO.TextReader" /> returning a list of comments and values.
/// </summary>
[<CompiledName("ParseTextReader")>]
let parseTextReader (textReader: TextReader) =
    let reader = Parser.textReaderToReader textReader
    Parser.parseWithReader reader

/// <summary>
/// Parse a Java '.properties' file from a <see cref="System.String" /> returning a list of comments and values.
/// </summary>
[<CompiledName("ParseString")>]
let parseString (s: string) =
    use reader = new StringReader(s)

    parseTextReader reader

/// <summary>
/// Parse a Java '.properties' file from a file by specifying it's path returning a list of comments and values.
/// </summary>
[<CompiledName("ParseFile")>]
let parseFile (path: string) =
    use stream = File.OpenRead(path)
    use reader = new StreamReader(stream)

    parseTextReader reader

/// <summary>
/// Convert a list of comments and values extracted from a Java '.properties' file to a map with only the values.
/// </summary>
[<CompiledName("ToMap")>]
let toMap (properties: Entry []) =
    properties
    |> Seq.choose (function | Comment _ -> None | KeyValue (k, v) -> Some (k, v))
    |> Map.ofSeq

/// <summary>
/// Convert a list of comments and values extracted from a Java '.properties' file to a dictionary with only the values.
/// </summary>
[<CompiledName("ToDictionary")>]
let toDictionary (properties: Entry []) =
    properties
    |> Seq.choose (function | Comment _ -> None | KeyValue (k, v) -> Some (k, v))
    |> dict
