### New in 0.6.0

* Added support for Mono on Unix specific encoding of `System.Process` arguments in `MonoUnixCommandLine`.
* Auto-detect Mono on Unix and switch to the specific implementation
* Added methods in `MonoUnixCommandLine` and `MsvcrCommandLine` to encode a single argument if needed
* Added tests to confirm that the current Msvcr handling work for .Net Core on Unix

### New in 0.5.1

* Msvcr: Methods now accept a settings record
* Msvcr: Added setting to use double quote escaping of quotes instead of backslash
* Msvcr: Added setting to always quote arguments

### New in 0.5.0

* Changed a big part of the `CmdLine` API, now with a lot more variants including prefixes and printf-style versions.

### New in 0.1.1

* Fixed a bug in `MsvcrCommandLine` where backslash characters not in front of a quote were incorrectly escaped. See [#1](https://github.com/vbfox/FoxSharp/issues/1), thanks @matthid.

### New in 0.1.0

* First version extracted from the old FoxSharp lib
