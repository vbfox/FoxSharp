module BlackFox.JavaPropertiesFile.JavaPropertiesFile

open System.IO
open System.Text

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

let private latin1 = lazy (Encoding.GetEncoding "ISO-8859-1")

/// <summary>
/// Parse a Java '.properties' file from a file by specifying it's path returning a list of comments and values.
/// <para>
/// The file is read as ISO-8859-1 (Latin-1), like <c>java.util.Properties.load(InputStream)</c> does. Characters
/// outside of Latin-1 are expected to be written as <c>\uXXXX</c> escapes. To read a file in another encoding
/// (like UTF-8) use <see cref="parseTextReader" /> with a <see cref="System.IO.StreamReader" /> created for that
/// encoding.
/// </para>
/// </summary>
[<CompiledName("ParseFile")>]
let parseFile (path: string) =
    use stream = File.OpenRead(path)

    // Same as java.util.Properties.load(InputStream): each byte is one Latin-1 character, no BOM detection.
    use reader = new StreamReader(stream, latin1.Value, false)

    parseTextReader reader

/// <summary>
/// Convert a list of comments and values extracted from a Java '.properties' file to a map with only the values.
/// </summary>
[<CompiledName("ToMap")>]
let toMap (properties: seq<Entry>) =
    properties
    |> Seq.choose (function | Comment _ -> None | KeyValue (k, v) -> Some (k, v))
    |> Map.ofSeq

/// <summary>
/// Convert a list of comments and values extracted from a Java '.properties' file to a dictionary with only the values.
/// </summary>
[<CompiledName("ToDictionary")>]
let toDictionary (properties: seq<Entry>) =
    properties
    |> Seq.choose (function | Comment _ -> None | KeyValue (k, v) -> Some (k, v))
    |> dict
