namespace BlackFox.CachedFSharpReflection

open System
open Microsoft.FSharp.Reflection
open System.Reflection

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

    let getTupleElements =
        DictCache.create
            (fun (t: Type) -> t.FullName)
            FSharpType.GetTupleElements

    let getFunctionElements =
        DictCache.create
            (fun (t: Type) -> t.FullName)
            FSharpType.GetFunctionElements

    let getRecordFields =
        DictCache.create
            (fun (t: Type) -> t.FullName)
            FSharpType.GetRecordFields

    let getUnionCases =
        DictCache.create
            (fun (t: Type) -> t.FullName)
            FSharpType.GetUnionCases

    let getExceptionFields =
        DictCache.create
            (fun (t: Type) -> t.FullName)
            FSharpType.GetExceptionFields

    static member private lazyShared = lazy (FSharpTypeCache())

    static member Shared with get() = FSharpTypeCache.lazyShared.Value

    /// <summary>Return true if the <c>typ</c> is a representation of an F# function type or the runtime type of a closure implementing an F# function type</summary>
    /// <param name="typ">The type to check.</param>
    /// <returns>True if the type check succeeds.</returns>
    member __.IsFunction (typ: Type): bool =
        isFunction |> DictCache.get typ

    /// <summary>Return true if the <c>typ</c> is a <c>System.Type</c> value corresponding to the compiled form of an F# module </summary>
    /// <param name="typ">The type to check.</param>
    /// <returns>True if the type check succeeds.</returns>
    member __.IsModule (typ: Type): bool =
        isModule |> DictCache.get typ

    /// <summary>Return true if the <c>typ</c> is a representation of an F# tuple type </summary>
    /// <param name="typ">The type to check.</param>
    /// <returns>True if the type check succeeds.</returns>
    member __.IsTuple (typ: Type): bool =
        isTuple |> DictCache.get typ

    /// <summary>Return true if the <c>typ</c> is a representation of an F# record type </summary>
    /// <param name="typ">The type to check.</param>
    /// <returns>True if the type check succeeds.</returns>
    member __.IsRecord (typ: Type): bool =
        isRecord |> DictCache.get typ

    /// <summary>Returns true if the <c>typ</c> is a representation of an F# union type or the runtime type of a value of that type</summary>
    /// <param name="typ">The type to check.</param>
    /// <returns>True if the type check succeeds.</returns>
    member __.IsUnion (typ: Type): bool =
        isUnion |> DictCache.get typ

    /// <summary>Returns true if the <c>typ</c> is a representation of an F# exception declaration</summary>
    /// <param name="exceptionType">The type to check.</param>
    /// <returns>True if the type check is an F# exception.</returns>
    member __.IsExceptionRepresentation (exceptionType: Type): bool =
        isExceptionRepresentation |> DictCache.get exceptionType

    /// <summary>Gets the tuple elements from the representation of an F# tuple type.</summary>
    /// <param name="tupleType">The input tuple type.</param>
    /// <returns>An array of the types contained in the given tuple type.</returns>
    member __.GetTupleElements(tupleType: Type):  Type[] =
        getTupleElements |> DictCache.get tupleType

    /// <summary>Gets the domain and range types from an F# function type  or from the runtime type of a closure implementing an F# type</summary>
    /// <param name="functionType">The input function type.</param>
    /// <returns>A tuple of the domain and range types of the input function.</returns>
    member __.GetFunctionElements(functionType: Type): Type * Type =
        getFunctionElements |> DictCache.get functionType

    /// <summary>Reads all the fields from a record value, in declaration order</summary>
    /// <remarks>Assumes the given input is a record value. If not, ArgumentException is raised.</remarks>
    /// <param name="recordType">The input record type.</param>
    /// <returns>An array of descriptions of the properties of the record type.</returns>
    member __.GetRecordFields(recordType: Type): PropertyInfo[] =
        getRecordFields |> DictCache.get recordType

    /// <summary>Gets the cases of a union type.</summary>
    /// <remarks>Assumes the given type is a union type. If not, ArgumentException is raised during pre-computation.</remarks>
    /// <param name="unionType">The input union type.</param>
    /// <exception cref="System.ArgumentException">Thrown when the input type is not a union type.</exception>
    /// <returns>An array of descriptions of the cases of the given union type.</returns>
    member __.GetUnionCases(unionType: Type): UnionCaseInfo[] =
        getUnionCases |> DictCache.get unionType

    /// <summary>Reads all the fields from an F# exception declaration, in declaration order</summary>
    /// <remarks>Assumes <c>exceptionType</c> is an exception representation type. If not, ArgumentException is raised.</remarks>
    /// <param name="exceptionType">The exception type to read.</param>
    /// <exception cref="System.ArgumentException">Thrown if the given type is not an exception.</exception>
    /// <returns>An array containing the PropertyInfo of each field in the exception.</returns>
    member __.GetExceptionFields(exceptionType: Type): PropertyInfo[] =
        getExceptionFields |> DictCache.get exceptionType

    member __.Clear() =
        DictCache.clear isFunction
        DictCache.clear isModule
        DictCache.clear isTuple
        DictCache.clear isRecord
        DictCache.clear isUnion
        DictCache.clear isExceptionRepresentation
        DictCache.clear getTupleElements
        DictCache.clear getFunctionElements
        DictCache.clear getRecordFields
        DictCache.clear getUnionCases
        DictCache.clear getExceptionFields
