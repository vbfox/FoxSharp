### New in 1.0.0

* Key the caches on the `Type` and `MemberInfo` instances instead of `Type.FullName` as Full names aren't unique
* Document that a cache keeps the types it has seen alive, and that `Shared` shouldn't be used for types loaded in a
  collectible `AssemblyLoadContext`
* Fix `Shared` returning a new cache instance on every access instead of a single shared one

### New in 0.1.1

* Add a class with both value and type cache that can be passed around

### New in 0.1.0

* First version of the Cached F# Reflection lib
