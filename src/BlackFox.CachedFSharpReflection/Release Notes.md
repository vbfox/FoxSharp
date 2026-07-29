### New in 0.2.0

* Key the caches on the `Type` and `MemberInfo` instances instead of `Type.FullName`. Full names aren't unique: two
  assemblies, two versions of an assembly or two `AssemblyLoadContext` can define the same one, and a cache lookup
  could return the pre-computed reader, constructor or field list of an unrelated type. They are also `null` for
  generic parameters, which made those throw `ArgumentNullException`
* Document that a cache keeps the types it has seen alive, and that `Shared` shouldn't be used for types loaded in a
  collectible `AssemblyLoadContext`

### New in 0.1.1

* Add a class with both value and type cache that can be passed around

### New in 0.1.0

* First version of the Cached F# Reflection lib
