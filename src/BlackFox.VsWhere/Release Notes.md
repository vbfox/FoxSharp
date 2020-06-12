### New in 1.1.0

* Add 2 new functions `getLegacy` and `getAllWithLegacy` that support versions before `ISetupInstance` existed
  (From Visual Studio .NET 2002 to Visual Studio 2015) via their registry keys
  (Thanks [@1354092549](https://github.com/1354092549))

### New in 1.0.0

* No changes but as FAKE is using the library the API shouldn't change now.

### New in 0.3.2

* Harden against null values in COM API. Fixes [#5](https://github.com/vbfox/FoxSharp/issues/5)
* Ignore instances that fail to be parsed (An error is logged to `System.Diagnostics.Trace`)

### New in 0.3.1

* Fix a bug in VsSetupPackage Version (The value was the ID instead of the version)

### New in 0.3.0

* Use F# lists (again)

### New in 0.2.4

* Package icon

### New in 0.2.3

* Add getCompleted

### New in 0.2.2

* Return an empty array on non-windows

### New in 0.2.1

* Add getWithPackage

### New in 0.2

* Return an array

### New in 0.1.1

* Build for net45
* Lower FSharp.Core version requirement

### New in 0.1.0

* First version with a simple API to enumerate all versions
