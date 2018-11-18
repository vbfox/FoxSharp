module BlackFox.FoxSharp.Tests.CmdLineTests

open BlackFox.CommandLine
open Expecto

[<Tests>]
let fullFiles =
    testList "CmdLine" [
        testCase "empty concat" <| fun () ->
            let r = CmdLine.concat [] |> CmdLine.toString
            Expect.equal r "" "toString"

        testCase "empty CmdLine concat" <| fun () ->
            let r = CmdLine.concat [CmdLine.empty] |> CmdLine.toString
            Expect.equal r "" "toString"

        testCase "Non-empty concat" <| fun () ->
            let r = CmdLine.concat [CmdLine.empty.Append("a").Append("b"); CmdLine.empty.Append("c").Append("d")] |> CmdLine.toString
            Expect.equal r "a b c d" "toString"
    ]
