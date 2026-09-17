module BlackFox.FoxSharp.Tests.CommandLineSpecialCharsTests

open Expecto
open Expecto.Flip
open BlackFox.CommandLine
open FsCheck
open FsCheck.FSharp

/// The default string generator rarely produces the characters that matter to the escaping rules. This one only
/// produces them, so that sequences like `\\"`, `""`, `'\'` or a trailing backslash are exercised in every run.
type SpecialChars = | SpecialChars of string

type SpecialCharsGenerator =
    static member Generator() =
        Gen.elements [ 'a'; ' '; '\t'; '"'; '\\'; '\''; '$'; '`'; '\n'; '\r' ]
        |> Gen.listOf
        |> Gen.map (fun chars -> SpecialChars (System.String(Array.ofList chars)))
        |> Arb.fromGen

let config = { FsCheckConfig.defaultConfig with arbitrary = [typeof<SpecialCharsGenerator>]; maxTest = 2000 }

let msvcrRoundtrips =
    testPropertyWithConfig config "MSVCR escape of special characters is the inverse of the .Net Core parser and of parse" <|
        fun (x: SpecialChars list) (alwaysQuoteArguments: bool) (doubleQuoteEscapeQuote: bool) ->
            let input = x |> List.map (fun (SpecialChars s) -> s)
            let settings =
                { MsvcrCommandLine.defaultEscapeSettings with
                    AlwaysQuoteArguments = alwaysQuoteArguments
                    DoubleQuoteEscapeQuote = doubleQuoteEscapeQuote }
            let escaped = MsvcrCommandLine.escape settings input
            Expect.equal (sprintf ".Net Core parser of %A" escaped) input (TestParsers.DotnetCoreUnix.Parse escaped |> List.ofArray)
            Expect.equal (sprintf "parse of %A" escaped) input (MsvcrCommandLine.parse escaped)

let monoRoundtrips =
    testPropertyWithConfig config "Mono escape of special characters is the inverse of the Mono parser and of parse" <|
        fun (x: SpecialChars list) (alwaysQuoteArguments: bool) ->
            let input = x |> List.map (fun (SpecialChars s) -> s)
            let settings =
                { MonoUnixCommandLine.defaultEscapeSettings with
                    AlwaysQuoteArguments = alwaysQuoteArguments }
            let escaped = MonoUnixCommandLine.escape settings input
            Expect.equal (sprintf "Mono parser of %A" escaped) input (TestParsers.MonoUnix.Parse escaped |> List.ofArray)
            Expect.equal (sprintf "parse of %A" escaped) input (MonoUnixCommandLine.parse escaped)

[<Tests>]
let test =
    testList "Command line special characters" [
        msvcrRoundtrips
        monoRoundtrips
    ]
