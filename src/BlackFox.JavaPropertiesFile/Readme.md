# Java .properties file parser

![Java Coffee Logo](https://raw.githubusercontent.com/vbfox/FoxSharp/master/src/BlackFox.JavaPropertiesFile/Icon.png)

[![Nuget Package](https://img.shields.io/nuget/v/BlackFox.JavaPropertiesFile.svg)](https://www.nuget.org/packages/BlackFox.JavaPropertiesFile)

A parser for Java `.properties` files that follows the format described in the
[`java.util.Properties.load(Reader)`][PropertiesLoad] documentation.

[PropertiesLoad]: https://docs.oracle.com/en/java/javase/21/docs/api/java.base/java/util/Properties.html#load(java.io.Reader)

## Usage

F#
```fsharp
open BlackFox.JavaPropertiesFile

let entries = JavaPropertiesFile.parseString "# Database\ndb.host = localhost\ndb.port = 5432"
// [ Comment " Database"; KeyValue ("db.host", "localhost"); KeyValue ("db.port", "5432") ]

let settings = JavaPropertiesFile.toMap entries
// map [ ("db.host", "localhost"); ("db.port", "5432") ]
```

C#
```csharp
using BlackFox.JavaPropertiesFile;

var entries = JavaPropertiesFile.ParseFile("app.properties");
IDictionary<string, string> settings = JavaPropertiesFile.ToDictionary(entries);
```

## API

Everything lives in the `BlackFox.JavaPropertiesFile` namespace. The functions are in the `JavaPropertiesFile` module,
which is a static class from C#.

### `Entry`

The parse functions return an `Entry list` (`FSharpList<Entry>` from C#, it implements `IEnumerable<Entry>`) with one
element per comment or key/value pair, in file order:

```fsharp
type Entry =
    | Comment of text : string
    | KeyValue of key : string * value : string
```

* `Comment` holds the text after the `#` or `!` marker, without the marker itself.
* `KeyValue` holds the key and value with all escapes already resolved. A key without a separator or value gives an
  empty string value, like Java.

### Parsing

| F# | C# | Description |
|---|---|---|
| `parseString (s: string)` | `ParseString(string)` | Parse the content of a `.properties` file held in a string. |
| `parseTextReader (reader: TextReader)` | `ParseTextReader(TextReader)` | Parse from any `TextReader`. The reader is not closed, the caller owns it. Use this to control the encoding. |
| `parseFile (path: string)` | `ParseFile(string)` | Open and parse a file, reading it as ISO-8859-1 (see [File encoding](#file-encoding)). |

A `\u` escape not followed by four hex digits throws an exception. Everything else is accepted, like Java.

### Converting

| F# | C# | Description |
|---|---|---|
| `toMap (entries: seq<Entry>)` | `ToMap(IEnumerable<Entry>)` | Drop the comments and build a `Map<string, string>`. |
| `toDictionary (entries: seq<Entry>)` | `ToDictionary(IEnumerable<Entry>)` | Drop the comments and build a read-only `IDictionary<string, string>`. |

When a key appears several times the last value wins, like `java.util.Properties`.

## File encoding

`parseFile` reads the file as **ISO-8859-1** (Latin-1), exactly like `java.util.Properties.load(InputStream)` does:
each byte is one character and no BOM detection is done. Characters outside of Latin-1 are expected to be written as
`\uXXXX` escapes, which the parser understands.

If your files are UTF-8 you can use `parseTextReader` like that:

```fsharp
open System.IO
open System.Text
open BlackFox.JavaPropertiesFile

let parseUtf8File (path: string) =
    use reader = new StreamReader(path, Encoding.UTF8)
    JavaPropertiesFile.parseTextReader reader
```
