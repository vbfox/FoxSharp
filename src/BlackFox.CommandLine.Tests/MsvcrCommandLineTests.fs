module BlackFox.CommandLine.Tests.MsvcrCommandLineEscapeTests

open Expecto

open BlackFox.CommandLine
open Expecto.Flip

let verifyEscape expected argv =
    let result = MsvcrCommandLine.escape argv
    Expect.equal expected expected result

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

    test "Odd_backslash_escaped" {
        verifyEscape @"""a\\\\b c"" d e" [@"a\\b c"; "d"; "e"]
    }

    test "Even_backslash_escaped" {
        verifyEscape @"""a\\\\\b c"" d e" [@"a\\\b c"; "d"; "e"]
    }

    test "Pass_empty_arguments" {
        verifyEscape @"a """" b" ["a"; ""; "b"]
    }
]

[<Tests>]
let test =
    testList "msvcr" [
        testList "escape" escapeTests
    ]