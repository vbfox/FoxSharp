# Cached F# Reflection

[![Nuget Package](https://img.shields.io/nuget/v/BlackFox.CachedFSharpReflection.svg)](https://www.nuget.org/packages/BlackFox.CachedFSharpReflection)

Cache the F# reflection API calls results for fast access

## Status

This library is an early preview.

[Change Log](Release%20Notes.md)

## Sample

```fsharp
open BlackFox.CachedFSharpReflection

type Foo = {
    Bar: int
}

// Use the shared cache
FSharpTypeCache.Shared.IsRecord(typeof<Foo>) // True
FSharpTypeCache.Shared.IsUnion(typeof<Foo>) // False

// Create a new cache
let cache = FSharpTypeCache()
cache.IsRecord(typeof<Foo>) // True
cache.IsUnion(typeof<Foo>) // False

```

## Cache lifetime

Entries are keyed on the `System.Type` and `System.Reflection.MemberInfo` instances themselves and are kept until
`Clear()` is called. A cache therefore keeps the types it has seen — and the assemblies that define them — alive for
as long as it lives, and `Shared` lives for as long as the process.

If your application loads assemblies that are meant to be unloaded again (a plugin host using a collectible
`AssemblyLoadContext`) don't use `Shared` for their types: give each load context its own cache instance and drop it
together with the context, or call `Clear()` when unloading.
