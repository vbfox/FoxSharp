### New in 0.1.3

* Add `runOrDefaultApp` and `runOrListApp` to use FAKE directly with `dotnet run` and still print the exception nicely
  colored and return a stable error code.
* Add an icon (orange version of FAKE one)

### New in 0.1.2

* Change `BuildTask.setupContextFromArgv` to take an array instead of a list

### New in 0.1.1

* `BuildTask.setupContextFromArgv` an API not specific to BuildTask but that make using FAKE from a normal program
  started via `dotnet run` easy.

### New in 0.1.0

* First version with this name and for FAKE 5
* Previous versions were maintained in [a Gist][previous_gist] and also in this repository as
  [untyped][previous_untyped] and [typed][previous_typed] variants.

[previous_gist]: https://gist.github.com/vbfox/e3e22d9ffff9b9de7f51
[previous_untyped]: https://github.com/vbfox/FoxSharp/blob/a42b65bbd53666ab51d7e621e9a41c6f8078218c/src/BlackFox.FakeUtils/TaskDefinitionHelper.fs
[previous_typed]: https://github.com/vbfox/FoxSharp/blob/a42b65bbd53666ab51d7e621e9a41c6f8078218c/src/BlackFox.FakeUtils/TypedTaskDefinitionHelper.fs
