module BlackFox.FoxSharp.Tests.MsvcrCommandLineEscapeTests

open Expecto
open Expecto.Flip
open BlackFox.CommandLine

let verifyEscape expected argv =
    let result = MsvcrCommandLine.escape argv
    Expect.equal (sprintf "%A" argv) expected result

let escapeTests = [
    test "Multiple_parameters_are_separated_by_spaces" {
        verifyEscape "Hello World" ["Hello"; "World"]
    }

    test "No_quotes_are_added_when_not_needed" {
        verifyEscape "Hello_World" ["Hello_World"]
    }

    test "Quotes_are_added_when_arg_contains_space" {
        verifyEscape @"""Hello World""" ["Hello World"]
    }

    test "Quote_is_escaped_inside" {
        verifyEscape @"Hello\""World" [@"Hello""World"]
    }

    test "Quote_is_escaped_at_start" {
        verifyEscape @"\""HelloWorld" [@"""HelloWorld"]
    }

    test "Quote_is_escaped_at_end" {
        verifyEscape @"HelloWorld\""" [@"HelloWorld"""]
    }

    test "Backslash_alone_not_escaped" {
        verifyEscape @"Hello\World" [@"Hello\World"]
    }

    test "Backslash_escaped_if_at_end_and_need_quote" {
        verifyEscape @"""Hello World\\""" [@"Hello World\"]
    }

    test "Backslash_not_escaped_if_at_end_and_no_need_to_need_quote" {
        verifyEscape @"Hello_World\" [@"Hello_World\"]
    }

    test "Backslash_before_quote_escaped" {
        verifyEscape @"Hello\\\""World" [@"Hello\""World"]
    }

    test "Microsoft_sample_1" {
        verifyEscape @"""a b c"" d e" ["a b c" ;"d" ;"e"]
    }

    test "Microsoft_sample_2" {
        verifyEscape @"ab\""c \ d" ["ab\"c" ;"\\" ;"d"]
    }

    test "Microsoft_sample_3_modified" {
        verifyEscape @"a\\\b ""de fg"" h" [@"a\\\b" ;"de fg" ;"h"]
    }

    test "Microsoft_sample_4" {
        verifyEscape @"a\\\""b c d" [@"a\""b" ;"c" ;"d"]
    }

    test "Microsoft_sample_5_modified" {
        verifyEscape @"""a\\b c"" d e" [@"a\\b c" ;"d" ;"e"]
    }

    test "Pass_empty_arguments" {
        verifyEscape @"a """" b" ["a"; ""; "b"]
    }
]

let verifyParse args expected =
    let result = MsvcrCommandLine.parse args
    Expect.equal args expected result

let parseTests = [
    test "Plain_parameter" {
        verifyParse @"CallMeIshmael" ["CallMeIshmael"]
    }

    test "Space_in_double_quoted" {
        verifyParse @"""Call Me Ishmael""" ["Call Me Ishmael"]
    }

    test "Double_quoted_anywhere" {
        verifyParse @"Cal""l Me I""shmael" ["Call Me Ishmael"]
    }

    test "Escape_quotes" {
        verifyParse @"CallMe\""Ishmael " [@"CallMe""Ishmael"]
    }

    test "Escape_backslash_end" {
        verifyParse @"""Call Me Ishmael\\""" [@"Call Me Ishmael\"]
    }

    test "Escape_backslash_middle" {
        verifyParse @"""CallMe\\\""Ishmael""" [@"CallMe\""Ishmael"]
    }

    test "Backslash_literal_without_quote" {
        verifyParse @"a\\\b" [@"a\\\b"]
    }

    test "Backslash_literal_without_quote_in_quoted" {
        verifyParse @"""a\\\b""" [@"a\\\b"]
    }

    test "Microsoft_sample_1" {
        verifyParse @"""a b c""  d  e" ["a b c" ;"d" ;"e"]
    }

    test "Microsoft_sample_2" {
        verifyParse @"""ab\""c""  ""\\""  d" ["ab\"c" ;"\\" ;"d"]
    }

    test "Microsoft_sample_3" {
        verifyParse @"a\\\b d""e f""g h" [@"a\\\b" ;"de fg" ;"h"]
    }

    test "Microsoft_sample_4" {
        verifyParse @"a\\\""b c d" [@"a\""b" ;"c" ;"d"]
    }

    test "Microsoft_sample_5" {
        verifyParse @"a\\\\""b c"" d e" [@"a\\b c" ;"d" ;"e"]
    }

    test "Double_double_quotes_sample_1" {
        verifyParse @"""a b c""""" [@"a b c"""]
    }

    test "Double_double_quotes_sample_2" {
        verifyParse @"""""""CallMeIshmael""""""  b  c " [@"""CallMeIshmael""" ;"b" ;"c"]
    }

    test "Double_double_quotes_sample_4" {
        verifyParse @"""""""""Call Me Ishmael"""" b c " [@"""Call" ;"Me" ;@"Ishmael" ;"b" ;"c"]
    }

    test "Triple_double_quotes" {
        verifyParse @"""""""Call Me Ishmael""""""" [@"""Call Me Ishmael"""]
    }

    test "Quadruple_double_quotes" {
        verifyParse @"""""""""Call me Ishmael""""""""" [@"""Call" ;"me" ;@"Ishmael"""]
    }
]

[<Tests>]
let test =
    testList "msvcr" [
        testList "escape" escapeTests
        testList "parse" parseTests
    ]

open FsCheck

[<Tests>]
let propertyBasedTests =
    testProperty "Escape is the inverse of Parse" <|
        fun (x: NonNull<string> list) ->
            let input = x |> List.map (fun (NonNull s) -> s)
            let escaped = MsvcrCommandLine.escape input
            let backAgain = MsvcrCommandLine.parse escaped
            Expect.equal "First Name should not be null" input backAgain
