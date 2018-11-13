module BlackFox.FoxSharp.Tests.DotnetCoreUnixCommandLineTests

open Expecto
open Expecto.Flip
open BlackFox.CommandLine
open FsCheck

[<Tests>]
let escapeRoundtripWithDotnetCore =
    testProperty "Escape is the inverse of .Net Core parse method" <|
        fun (x: NonNull<string> list) (alwaysQuoteArguments: bool) ->
            let input = x |> List.map (fun (NonNull s) -> s)
            let settings =
                { MsvcrCommandLine.defaultEscapeSettings with
                    AlwaysQuoteArguments = alwaysQuoteArguments }
            let escaped = MsvcrCommandLine.escape settings input
            let backAgain = TestParsers.DotnetCoreUnix.Parse escaped |> List.ofArray
            Expect.equal "Input and escaped/parsed should equal" input backAgain

[<Tests>]
let test =
    testList ".NET Core command line" [
        testList "Property Based" [escapeRoundtripWithDotnetCore]
    ]
