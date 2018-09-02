module BlackFox.MasterOfFoo.Build.Program

open BlackFox.Fake
open Fake.Core
open Fake.BuildServer

[<EntryPoint>]
let main argv =
    BuildTask.setupContextFromArgv argv
    BuildServer.install [ AppVeyor.Installer ]

    let defaultTask = Tasks.createAndGetDefault ()
    BuildTask.runOrDefault defaultTask
    0
