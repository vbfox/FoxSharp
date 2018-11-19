module BlackFox.FoxSharp.Tests.MonoUnixCommandLineTests

open Expecto
open Expecto.Flip
open BlackFox.CommandLine
open FsCheck

type NotZeroChar = | NotZeroChar of string

type NotZeroCharGenerator =
    static member Generator() =
        Arb.from<string>
        |> Arb.filter (fun (s: string) -> s <> null && not (s.ToCharArray() |> Array.contains '\x00'))
        |> Arb.convert NotZeroChar (fun (NotZeroChar s) -> s)

let config = { FsCheckConfig.defaultConfig with arbitrary = [typeof<NotZeroCharGenerator>] }

let escapeRoundtripWithMono =
    testPropertyWithConfig config "Escape is the inverse of Mono parse method" <|
        fun (x: NotZeroChar list) (alwaysQuoteArguments: bool) ->
            let input = x |> List.map (fun (NotZeroChar s) -> s)
            let settings =
                { MonoUnixCommandLine.defaultEscapeSettings with
                    AlwaysQuoteArguments = alwaysQuoteArguments }
            let escaped = MonoUnixCommandLine.escape settings input
            try
                let backAgain = TestParsers.MonoUnix.Parse escaped |> List.ofArray
                Expect.equal "Input and escaped/parsed should equal" input backAgain
            with
            | _ ->
                printfn "======"
                escaped |> Seq.map (fun c -> (int c).ToString("X2")) |> String.concat " " |> printfn "INPUTBYTES=%s"
                printfn "INPUT=%A" input
                printfn "ESCAPED==%s==" escaped
                reraise ()

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
        testList "Property Based" [
            escapeRoundtripWithMono
            escapeRoundtripWithParse
        ]
    ]
