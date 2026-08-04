module BlackFox.CommandLine.Tests.Main

open Expecto

[<EntryPoint>]
let main args =
    runTestsInAssemblyWithCLIArgs [ NUnit_Summary "TestResults.xml" ] args
