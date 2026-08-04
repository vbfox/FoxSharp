module BlackFox.FoxSharp.Tests.JavaPropertiesFileTests

open Expecto
open Expecto.Flip
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

[<Tests>]
let fromDoc =
    testList "From doc" [
        testCase "Fruits" <| fun () ->
            let file = @"
fruits                           apple, banana, pear, \
                                    cantaloupe, watermelon, \
                                    kiwi, mango"
            let parsed = JavaPropertiesFile.parseString file
            let expected =
                [
                    KeyValue("fruits", "apple, banana, pear, cantaloupe, watermelon, kiwi, mango")
                ]

            Expect.equal "eq" expected parsed

        testCase "Truth" <| fun () ->
            let file = @"
Truth = Beauty
       Truth:Beauty
Truth                  :Beauty"
            let parsed = JavaPropertiesFile.parseString file
            let expected =
                [
                    KeyValue("Truth", "Beauty")
                    KeyValue("Truth", "Beauty")
                    KeyValue("Truth", "Beauty")
                ]

            Expect.equal "eq" expected parsed

        testCase "Cheeses" <| fun () ->
            let file = @"
cheeses"
            let parsed = JavaPropertiesFile.parseString file
            let expected =
                [
                    KeyValue("cheeses", "")
                ]

            Expect.equal "eq" expected parsed

        testCase "all escaped" <| fun () ->
            let file = @"
\:\= \:\=\r\nfo\o\u0020bar\t\f"
            let parsed = JavaPropertiesFile.parseString file
            let expected =
                [
                    KeyValue(":=", ":=\r\nfoo bar\t\u000c")
                ]

            Expect.equal "eq" expected parsed

        testCase "comments" <| fun () ->
            let file = @"
#Hello
   !World\
key=value"
            let parsed = JavaPropertiesFile.parseString file
            let expected =
                [
                    Comment("Hello")
                    Comment("World\\")
                    KeyValue("key", "value")
                ]

            Expect.equal "eq" expected parsed
        ]

[<Tests>]
let specialCases =
    testList "Special cases" [
        testCase "Space separator" <| fun () ->
            let parsed = JavaPropertiesFile.parseString "foo bar"
            let expected = [ KeyValue("foo", "bar") ]

            Expect.equal "eq" expected parsed

        testCase "Equal separator" <| fun () ->
            let parsed = JavaPropertiesFile.parseString "foo=bar"
            let expected = [ KeyValue("foo", "bar") ]

            Expect.equal "eq" expected parsed

        testCase "Two points separator" <| fun () ->
            Expect.equal
                "eq"
                ([ KeyValue("foo", "bar") ])
                (JavaPropertiesFile.parseString "foo:bar")

        testCase "Ends with escape" <| fun () ->
            Expect.equal
                "eq"
                ([ KeyValue("foo", "bar") ])
                (JavaPropertiesFile.parseString "foo:bar\\\r\n")

        testCase "Escaped tab in key" <| fun () ->
            Expect.equal
                "eq"
                ([ KeyValue("fo\to", "bar") ])
                (JavaPropertiesFile.parseString "fo\\to:bar")

        testCase "Escaped tab in value" <| fun () ->
            Expect.equal
                "eq"
                ([ KeyValue("foo", "ba\tr") ])
                (JavaPropertiesFile.parseString "foo:ba\\tr")

        testCase "Unicode in key" <| fun () ->
            Expect.equal
                "eq"
                ([ KeyValue("\uD83D\udc0e", "") ])
                (JavaPropertiesFile.parseString "\\uD83D\\udc0e")

        testCase "Unicode in value" <| fun () ->
            Expect.equal
                "eq"
                ([ KeyValue("foo", "\uD83D\udc0e") ])
                (JavaPropertiesFile.parseString "foo:\\uD83D\\udc0e")
    ]

[<Tests>]
let blankContinuationLine =
    testList "Blank line after line continuation" [
        testCase "Whitespace-only continuation line (spaces) doesn't truncate the rest of the file" <| fun () ->
            let file = "a=1\\\n   \nrole=admin\nssl=on"
            let parsed = JavaPropertiesFile.parseString file
            let expected =
                [
                    KeyValue("a", "1")
                    KeyValue("role", "admin")
                    KeyValue("ssl", "on")
                ]

            Expect.equal "eq" expected parsed

        testCase "Whitespace-only continuation line (tabs) doesn't truncate the rest of the file" <| fun () ->
            let file = "a=1\\\n\t\t\nrole=admin\nssl=on"
            let parsed = JavaPropertiesFile.parseString file
            let expected =
                [
                    KeyValue("a", "1")
                    KeyValue("role", "admin")
                    KeyValue("ssl", "on")
                ]

            Expect.equal "eq" expected parsed

        testCase "Whitespace-only continuation line (CRLF) doesn't truncate the rest of the file" <| fun () ->
            let file = "a=1\\\r\n   \r\nrole=admin\r\nssl=on"
            let parsed = JavaPropertiesFile.parseString file
            let expected =
                [
                    KeyValue("a", "1")
                    KeyValue("role", "admin")
                    KeyValue("ssl", "on")
                ]

            Expect.equal "eq" expected parsed

        testCase "Whitespace-only continuation line at end of file" <| fun () ->
            let file = "a=1\\\n   "
            let parsed = JavaPropertiesFile.parseString file
            let expected = [ KeyValue("a", "1") ]

            Expect.equal "eq" expected parsed
    ]

[<Tests>]
let javaParity =
    testList "Parity with java.util.Properties" [
        testCase "Form feed acts as a key/value separator like space and tab" <| fun () ->
            let parsed = JavaPropertiesFile.parseString "foo\fbar"
            let expected = [ KeyValue("foo", "bar") ]

            Expect.equal "eq" expected parsed

        testCase "Tab acts as a key/value separator like space" <| fun () ->
            let parsed = JavaPropertiesFile.parseString "foo\tbar"
            let expected = [ KeyValue("foo", "bar") ]

            Expect.equal "eq" expected parsed

        testCase "Form feed is stripped as leading whitespace on a continuation line" <| fun () ->
            let file = "a=1\\\n\f\f\nrole=admin"
            let parsed = JavaPropertiesFile.parseString file
            let expected =
                [
                    KeyValue("a", "1")
                    KeyValue("role", "admin")
                ]

            Expect.equal "eq" expected parsed

        testCase "Escaped space in a key is kept literally instead of ending the key" <| fun () ->
            let parsed = JavaPropertiesFile.parseString "fo\\ o:bar"
            let expected = [ KeyValue("fo o", "bar") ]

            Expect.equal "eq" expected parsed

        testCase "Escaped tab in a key is kept literally instead of ending the key" <| fun () ->
            let parsed = JavaPropertiesFile.parseString "fo\\\to:bar"
            let expected = [ KeyValue("fo\to", "bar") ]

            Expect.equal "eq" expected parsed

        testCase "Only the first separator is consumed, a second one is part of the value" <| fun () ->
            let parsed = JavaPropertiesFile.parseString "key::value"
            let expected = [ KeyValue("key", ":value") ]

            Expect.equal "eq" expected parsed

        testCase "Only the first separator is consumed even with whitespace around it" <| fun () ->
            let parsed = JavaPropertiesFile.parseString "key  ::  value"
            let expected = [ KeyValue("key", ":  value") ]

            Expect.equal "eq" expected parsed

        testCase "Line continuation is honored while still parsing the key" <| fun () ->
            let parsed = JavaPropertiesFile.parseString "ke\\\ny=value"
            let expected = [ KeyValue("key", "value") ]

            Expect.equal "eq" expected parsed

        testCase "Line continuation while parsing the key strips leading whitespace of the next line" <| fun () ->
            let parsed = JavaPropertiesFile.parseString "ke\\\n   y=value"
            let expected = [ KeyValue("key", "value") ]

            Expect.equal "eq" expected parsed
    ]

[<Tests>]
let fullFiles =
    testList "Fullfiles" [
        testCase "1" <| fun () ->
            let parsed = JavaPropertiesFile.parseString @"#Fri Jan 17 22:37:45 MYT 2014
dbpassword=password
database=localhost
dbuser=vbfox"
            let expected =
                [
                    Comment("Fri Jan 17 22:37:45 MYT 2014")
                    KeyValue("dbpassword", "password")
                    KeyValue("database", "localhost")
                    KeyValue("dbuser", "vbfox")
                ]

            Expect.equal "eq" expected parsed
    ]
