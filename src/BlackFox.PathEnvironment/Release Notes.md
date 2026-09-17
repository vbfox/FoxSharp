### New in 0.3.1

* Fix a bug where `findExecutable`/`findFile` would still search the current directory even when `includeCurrentDirectory` was `false`, whenever `PATH` had a leading, trailing or doubled separator (a trailing `;` is common on Windows). Empty entries in `PATH`/`PATHEXT` are now dropped.
* Fix `path`/`pathExt` throwing a `NullReferenceException` when `PATH`/`PATHEXT` isn't set.
* On Windows, `pathExt` now falls back to the list built into `cmd.exe` (`.COM;.EXE;.BAT;.CMD;.VBS;.JS;.WS;.MSC`) when `PATHEXT` isn't set, so `findExecutable` can still find programs instead of never matching anything.
* `findExecutable`/`findFile` no longer throw when a name can't be combined into a path; such candidates are now skipped instead.

### New in 0.3.0

* Fix bug on windows where files without extensions were considered executable

### New in 0.2.0

* Nicer API

### New in 0.1.0

* First version extracted from the old FoxSharp lib
