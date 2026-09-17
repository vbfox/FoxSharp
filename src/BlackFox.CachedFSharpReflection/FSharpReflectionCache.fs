namespace BlackFox.CachedFSharpReflection

type FSharpReflectionCache(valueCache: FSharpValueCache, typeCache: FSharpTypeCache) =
    static let lazyShared = lazy (FSharpReflectionCache(FSharpValueCache.Shared, FSharpTypeCache.Shared))

    new() = FSharpReflectionCache(FSharpValueCache(), FSharpTypeCache())

    static member Shared with get() = lazyShared.Value

    member __.FSharpValue with get() = valueCache
    member __.FSharpType with get () = typeCache
