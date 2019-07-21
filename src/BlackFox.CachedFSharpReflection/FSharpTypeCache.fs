namespace BlackFox.CachedFSharpReflection

open System
open Microsoft.FSharp.Reflection

type FSharpTypeCache() =
    let isFunction =
        DictCache.create
            (fun (t: Type) -> t.FullName)
            FSharpType.IsFunction

    let isModule =
        DictCache.create
            (fun (t: Type) -> t.FullName)
            FSharpType.IsModule

    let isTuple =
        DictCache.create
            (fun (t: Type) -> t.FullName)
            FSharpType.IsTuple

    let isRecord =
        DictCache.create
            (fun (t: Type) -> t.FullName)
            FSharpType.IsRecord

    let isUnion =
        DictCache.create
            (fun (t: Type) -> t.FullName)
            FSharpType.IsUnion

    let isExceptionRepresentation =
        DictCache.create
            (fun (t: Type) -> t.FullName)
            FSharpType.IsExceptionRepresentation

    static member private lazyShared = lazy (FSharpTypeCache())

    static member Shared with get() = FSharpTypeCache.lazyShared.Value

    member __.IsFunction (t: Type): bool =
        isFunction |> DictCache.get t

    member __.IsModule (t: Type): bool =
        isModule |> DictCache.get t

    member __.IsTuple (t: Type): bool =
        isTuple |> DictCache.get t

    member __.IsRecord (t: Type): bool =
        isRecord |> DictCache.get t

    member __.IsUnion (t: Type): bool =
        isUnion |> DictCache.get t

    member __.IsExceptionRepresentation (t: Type): bool =
        isExceptionRepresentation |> DictCache.get t
