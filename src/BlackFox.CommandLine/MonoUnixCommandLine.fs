/// Parse and escape arguments as Mono does it on Unix platforms in the System.Process type
module BlackFox.CommandLine.MonoUnixCommandLine

/// Settings for the escape method
type EscapeSettings = {
    /// Specify that arguments should always be quoted, even simple values
    AlwaysQuoteArguments: bool
}

/// Default escape settings
let defaultEscapeSettings = {
    AlwaysQuoteArguments = false }

let escape (settings: EscapeSettings) cmdLine =
    let msvcrSettings =
        { MsvcrCommandLine.defaultEscapeSettings with
            AlwaysQuoteArguments = settings.AlwaysQuoteArguments }
    MsvcrCommandLine.escape msvcrSettings cmdLine

let parse (args: string) =
    []
