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
