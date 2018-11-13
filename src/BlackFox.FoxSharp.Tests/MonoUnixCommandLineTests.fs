module BlackFox.FoxSharp.Tests.MonoUnixCommandLineTests

open Expecto
open Expecto.Flip
open BlackFox.CommandLine
open FsCheck

let escapeRoundtripWithMono =
    testProperty "Escape is the inverse of Mono parse method" <|
        fun (x: NonNull<string> list) (alwaysQuoteArguments: bool) ->
            let input = x |> List.map (fun (NonNull s) -> s)
            let settings =
                { MonoUnixCommandLine.defaultEscapeSettings with
                    AlwaysQuoteArguments = alwaysQuoteArguments }
            let escaped = MonoUnixCommandLine.escape settings input
            let backAgain = TestParsers.MonoUnix.Parse escaped |> List.ofArray
            Expect.equal "Input and escaped/parsed should equal" input backAgain

let escapeRoundtripWithParse =
    testProperty "Escape is the inverse of parse method" <|
        fun (x: NonNull<string> list) (alwaysQuoteArguments: bool) ->
            let input = x |> List.map (fun (NonNull s) -> s)
            let settings =
                { MonoUnixCommandLine.defaultEscapeSettings with
                    AlwaysQuoteArguments = alwaysQuoteArguments }
            let escaped = MonoUnixCommandLine.escape settings input
            let backAgain = MonoUnixCommandLine.parse escaped
            Expect.equal "Input and escaped/parsed should equal" input backAgain


[<Tests>]
let test =
    testList "Mono unix command line" [
        //testList "escape" escapeTests
        //testList "parse" parseTests
        testList "Property Based" [
            escapeRoundtripWithMono
            escapeRoundtripWithParse
        ]
    ]
