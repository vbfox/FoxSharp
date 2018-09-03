module BlackFox.FoxSharp.Tests.JavaPropertiesFileTests

open Expecto
open Expecto.Flip
open BlackFox
open BlackFox.JavaPropertiesFile

[<Tests>]
let empty = test "Parse empty" {
    let expected = List.empty
    let actual = JavaPropertiesFile.parseString ""
    Expect.equal "" expected actual
}

[<Tests>]
let simple = test "Parse simple" {
    let expected = [
        Comment "Hello world"
        KeyValue ("key", "value")
    ]
    let actual = JavaPropertiesFile.parseString "#Hello world\nkey=value"
    Expect.equal "" expected actual
}

[<Tests>]
let twoPoints = test "Parse :" {
    let expected = [
        KeyValue ("key", "value")
    ]
    let actual = JavaPropertiesFile.parseString "key:value"
    Expect.equal "" expected actual
}

[<Tests>]
let ignoreWhiteSpace = test "Ignore whitespace in the middle" {
    let expected = [
        KeyValue ("key", "value")
    ]
    let actual = JavaPropertiesFile.parseString "key  :   value"
    Expect.equal "" expected actual
}

[<Tests>]
let endWhiteSpace = test "End whitespace is kept as part of the value" {
    let expected = [
        KeyValue ("key", "value   ")
    ]
    let actual = JavaPropertiesFile.parseString "key:value   "
    Expect.equal "" expected actual
}

[<Tests>]
let multipleLines = test "Property can span multiple lines" {
    let expected = [
        KeyValue ("key", "Hello World")
    ]
    let actual = JavaPropertiesFile.parseString "key=\\\n    Hello \\\n     World"
    Expect.equal "" expected actual
}

[<Tests>]
let specialChars = test "Special characters can be present" {
    let expected = [
        KeyValue ("key", "Hello\nWorld\t!\r")
    ]
    let actual = JavaPropertiesFile.parseString "key=Hello\\nWorld\\t!\\r"
    Expect.equal "" expected actual
}

[<Tests>]
let escapedBackslash = test "Escaped backslash works" {
    let expected = [
        KeyValue ("key", "Hello\\World")
    ]
    let actual = JavaPropertiesFile.parseString "key=Hello\\\\World"
    Expect.equal "" expected actual
}

[<Tests>]
let unicode = test "Unicode can be escaped" {
    let expected = [
        KeyValue ("key", "Hello\u0001World")
    ]
    let actual = JavaPropertiesFile.parseString "key=Hello\\u0001World"
    Expect.equal "" expected actual
}
