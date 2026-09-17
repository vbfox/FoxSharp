### New in 0.3.1

* `toMap` and `toDictionary` now accept any `seq<Entry>` (`IEnumerable<Entry>`), so the `Entry list` returned by the parse functions can be passed to them directly.
* **Breaking**: `parseFile` now reads the file as ISO-8859-1 (Latin-1) without BOM detection, like `java.util.Properties.load(InputStream)`, instead of UTF-8.
* Fix a bug where a line continuation (`\`) followed by a blank or whitespace-only line silently truncated the rest of the file instead of continuing to parse it.
* Fix several parsing edge cases to match `java.util.Properties` more closely: form feed is now treated as whitespace (it previously wasn't, and an unrelated character wrongly was), tab and form feed now act as key/value separators like space, an escaped whitespace character in a key is kept literally instead of ending the key, only a single `:`/`=` separator is consumed (a second one becomes part of the value), and a line continuation is now honored while still parsing the key part of a line.

### New in 0.3.0

* Back to F# Lists

### New in 0.2.0

* Cleanup the API and make it a little more C# friendly
* Unit tests

### New in 0.1.1

* Build for net45
* Lower FSharp.Core version requirement

### New in 0.1.0

* First version extracted from FAKE source code
