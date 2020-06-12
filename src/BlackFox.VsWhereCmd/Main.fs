module BlackFox.VsWhereCmd

open BlackFox.VsWhere

[<EntryPoint>]
let main _args =
    for i in VsInstances.getAllWithLegacy() do
        printfn "%s - %s" i.DisplayName i.InstallationVersion
        printfn "    %s" i.InstallationPath
        printfn ""

    0
