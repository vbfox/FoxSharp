### New in 0.3.1

* Fix a bug where `findExecutable`/`findFile` would still search the current directory even when `includeCurrentDirectory` was `false`, whenever `PATH` had a leading, trailing or doubled separator (a trailing `;` is common on Windows). Empty entries in `PATH`/`PATHEXT` are now dropped.
* Fix `path`/`pathExt` throwing a `NullReferenceException` instead of returning an empty array when `PATH`/`PATHEXT` isn't set.
* `findExecutable`/`findFile` no longer throw when a name can't be combined into a path; such candidates are now skipped instead.
* Document that `includeCurrentDirectory` searches the current directory before the PATH, and that names are
  used as relative paths so one containing a directory separator can escape the PATH directories.
* Fix `findExecutable` on windows not finding a name that already has an executable extension
  (`findExecutable "node.exe"` looked for `node.exe.COM`, `node.exe.EXE`, … but never for `node.exe`). The name is
  now also tried as-is when it ends with one of the `PATHEXT` extensions, as `cmd.exe` does. A name with another
  extension is still not considered executable.

### New in 0.3.0

* Fix bug on windows where files without extensions were considered executable

### New in 0.2.0

* Nicer API

### New in 0.1.0

* First version extracted from the old FoxSharp lib
