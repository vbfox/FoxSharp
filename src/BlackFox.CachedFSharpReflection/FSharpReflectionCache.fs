namespace BlackFox.CachedFSharpReflection

type FSharpReflectionCache(valueCache: FSharpValueCache, typeCache: FSharpTypeCache) =
    new() = FSharpReflectionCache(FSharpValueCache(), FSharpTypeCache())

    static member private lazyShared = lazy (FSharpReflectionCache(FSharpValueCache.Shared, FSharpTypeCache.Shared))
    static member Shared with get() = FSharpReflectionCache.lazyShared.Value

    member __.FSharpValue with get() = valueCache
    member __.FSharpType with get () = typeCache
