﻿module BlackFox.MasterOfFoo.Build.Program

open BlackFox.Fake
open Fake.Core
open Fake.BuildServer

[<EntryPoint>]
let main argv =
    BuildTask.setupContextFromArgv argv
    BuildServer.install [ TeamFoundation.Installer ]

    let defaultTask = Tasks.createAndGetDefault ()
    BuildTask.runOrDefaultApp defaultTask
