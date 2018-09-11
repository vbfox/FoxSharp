module BlackFox.CommandLine.Tests.Main

open Expecto

[<EntryPoint>]
let main args =
    let writeResults = TestResults.writeNUnitSummary ("TestResults.xml", "BlackFox.MasterOfFoo.Tests")
    let config = defaultConfig.appendSummaryHandler writeResults
    runTestsInAssembly config args
