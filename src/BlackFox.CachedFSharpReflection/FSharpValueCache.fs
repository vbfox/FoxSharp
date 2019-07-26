namespace BlackFox.CachedFSharpReflection

open System
open Microsoft.FSharp.Reflection
open System.Reflection

type FSharpValueCache() =
    let typeFullName (t: Type) =
        t.FullName

    let propertyFullName (p: PropertyInfo) =
        p.DeclaringType.FullName + "." + p.Name

    let unionCaseFullName (c: UnionCaseInfo) =
        c.DeclaringType.FullName + "." + c.Name

    let recordFieldReader =
        DictCache.create
            propertyFullName
            FSharpValue.PreComputeRecordFieldReader

    let recordReader =
        DictCache.create
            typeFullName
            FSharpValue.PreComputeRecordReader

    let recordConstructor =
        DictCache.create
            typeFullName
            FSharpValue.PreComputeRecordConstructor

    let recordConstructorInfo =
        DictCache.create
            typeFullName
            FSharpValue.PreComputeRecordConstructorInfo

    let unionTagReader =
        DictCache.create
            typeFullName
            FSharpValue.PreComputeUnionTagReader

    let unionTagMemberInfo =
        DictCache.create
            typeFullName
            FSharpValue.PreComputeUnionTagMemberInfo

    let unionConstructorInfo =
        DictCache.create
            unionCaseFullName
            FSharpValue.PreComputeUnionConstructorInfo

    let unionConstructor =
        DictCache.create
            unionCaseFullName
            FSharpValue.PreComputeUnionConstructor

    let unionReader =
        DictCache.create
            unionCaseFullName
            FSharpValue.PreComputeUnionReader

    let tupleReader =
        DictCache.create
            typeFullName
            FSharpValue.PreComputeTupleReader

    let tuplePropertyInfo =
        DictCache.create
            (fun struct(tupleType: Type, index: int) -> sprintf "%s.%i" tupleType.FullName index)
            (fun struct(tupleType: Type, index: int) -> FSharpValue.PreComputeTuplePropertyInfo(tupleType, index))

    let tupleConstructor =
        DictCache.create
            typeFullName
            FSharpValue.PreComputeTupleConstructor

    let tupleConstructorInfo =
        DictCache.create
            typeFullName
            FSharpValue.PreComputeTupleConstructorInfo

    /// <summary>Precompute a function for reading a particular field from a record.
    /// Assumes the given type is a RecordType with a field of the given name. 
    /// If not, ArgumentException is raised during pre-computation.</summary>
    ///
    /// <remarks>Using the computed function will typically be faster than executing a corresponding call to Value.GetInfo
    /// because the path executed by the computed function is optimized given the knowledge that it will be
    /// used to read values of the given type.</remarks>
    /// <param name="info">The PropertyInfo of the field to read.</param>
    /// <exception cref="System.ArgumentException">Thrown when the input type is not a record type.</exception>
    /// <returns>A function to read the specified field from the record.</returns>
    member __.GetRecordFieldReader (info: PropertyInfo) : (obj -> obj) =
        recordFieldReader |> DictCache.get info

    /// <summary>Precompute a function for reading all the fields from a record. The fields are returned in the
    /// same order as the fields reported by a call to Microsoft.FSharp.Reflection.Type.GetInfo for
    /// this type.</summary>
    ///
    /// <remarks>Assumes the given type is a RecordType. 
    /// If not, ArgumentException is raised during pre-computation.
    ///
    /// Using the computed function will typically be faster than executing a corresponding call to Value.GetInfo
    /// because the path executed by the computed function is optimized given the knowledge that it will be
    /// used to read values of the given type.</remarks>
    /// <param name="recordType">The type of record to read.</param>
    /// <exception cref="System.ArgumentException">Thrown when the input type is not a record type.</exception>
    /// <returns>An optimized reader for the given record type.</returns>
    member __.GetRecordReader (recordType: Type) : (obj -> obj[]) =
        recordReader |> DictCache.get recordType

    /// <summary>Precompute a function for constructing a record value. </summary>
    ///
    /// <remarks>Assumes the given type is a RecordType.
    /// If not, ArgumentException is raised during pre-computation.</remarks>
    /// <param name="recordType">The type of record to construct.</param>
    /// <exception cref="System.ArgumentException">Thrown when the input type is not a record type.</exception>
    /// <returns>A function to construct records of the given type.</returns>
    member __.GetRecordConstructor (recordType:Type) : (obj[] -> obj) =
        recordConstructor |> DictCache.get recordType

    /// <summary>Get a ConstructorInfo for a record type</summary>
    /// <param name="recordType">The record type.</param>
    /// <returns>A ConstructorInfo for the given record type.</returns>
    member __.GetRecordConstructorInfo (recordType:Type) : ConstructorInfo =
        recordConstructorInfo |> DictCache.get recordType

    /// <summary>Assumes the given type is a union type. 
    /// If not, ArgumentException is raised during pre-computation.</summary>
    ///
    /// <remarks>Using the computed function is more efficient than calling GetUnionCase
    /// because the path executed by the computed function is optimized given the knowledge that it will be
    /// used to read values of the given type.</remarks>
    /// <param name="unionType">The type of union to optimize reading.</param>
    /// <returns>An optimized function to read the tags of the given union type.</returns>
    member __.GetUnionTagReader (unionType: Type) : (obj -> int) =
        unionTagReader |> DictCache.get unionType

    /// <summary>Precompute a property or static method for reading an integer representing the case tag of a union type.</summary>
    /// <param name="unionType">The type of union to read.</param>
    /// <returns>The description of the union case reader.</returns>
    member __.GetUnionTagMemberInfo (unionType: Type) : MemberInfo =
        unionTagMemberInfo |> DictCache.get unionType

    /// <summary>Precompute a function for reading all the fields for a particular discriminator case of a union type</summary>
    ///
    /// <remarks>Using the computed function will typically be faster than executing a corresponding call to GetFields</remarks>
    /// <param name="unionCase">The description of the union case to read.</param>
    /// <returns>A function to for reading the fields of the given union case.</returns>
    member __.GetUnionReader (unionCase: UnionCaseInfo): (obj -> obj[]) =
        unionReader |> DictCache.get unionCase

    /// <summary>Precompute a function for constructing a discriminated union value for a particular union case. </summary>
    /// <param name="unionCase">The description of the union case.</param>
    /// <returns>A function for constructing values of the given union case.</returns>
    member __.GetUnionConstructor (unionCase:UnionCaseInfo): (obj[] -> obj) =
        unionConstructor |> DictCache.get unionCase

    /// <summary>A method that constructs objects of the given case</summary>
    /// <param name="unionCase">The description of the union case.</param>
    /// <returns>The description of the constructor of the given union case.</returns>
    member __.GetUnionConstructorInfo (unionCase:UnionCaseInfo): MethodInfo =
        unionConstructorInfo |> DictCache.get unionCase

    /// <summary>Precompute a function for reading the values of a particular tuple type</summary>
    ///
    /// <remarks>Assumes the given type is a TupleType.
    /// If not, ArgumentException is raised during pre-computation.</remarks>
    /// <param name="tupleType">The tuple type to read.</param>
    /// <exception cref="System.ArgumentException">Thrown when the given type is not a tuple type.</exception>
    /// <returns>A function to read values of the given tuple type.</returns>
    member __.GetTupleReader(tupleType:Type): (obj -> obj[]) =
        tupleReader |> DictCache.get tupleType
    
    /// <summary>Gets information that indicates how to read a field of a tuple</summary>
    /// <param name="tupleType">The input tuple type.</param>
    /// <param name="index">The index of the tuple element to describe.</param>
    /// <returns>The description of the tuple element and an optional type and index if the tuple is big.</returns>
    member __.GetTuplePropertyInfo(tupleType:Type, index:int): PropertyInfo * (Type * int) option =
        tuplePropertyInfo |> DictCache.get struct(tupleType, index)
    
    /// <summary>Precompute a function for reading the values of a particular tuple type</summary>
    ///
    /// <remarks>Assumes the given type is a TupleType.
    /// If not, ArgumentException is raised during pre-computation.</remarks>
    /// <param name="tupleType">The type of tuple to read.</param>
    /// <exception cref="System.ArgumentException">Thrown when the given type is not a tuple type.</exception>
    /// <returns>A function to read a particular tuple type.</returns>
    member __.GetTupleConstructor(tupleType:Type): (obj[] -> obj) =
        tupleConstructor |> DictCache.get tupleType

    /// <summary>Gets a method that constructs objects of the given tuple type. 
    /// For small tuples, no additional type will be returned.</summary>
    /// 
    /// <remarks>For large tuples, an additional type is returned indicating that
    /// a nested encoding has been used for the tuple type. In this case
    /// the suffix portion of the tuple type has the given type and an
    /// object of this type must be created and passed as the last argument 
    /// to the ConstructorInfo. A recursive call to PreComputeTupleConstructorInfo 
    /// can be used to determine the constructor for that the suffix type.</remarks>
    /// <param name="tupleType">The input tuple type.</param>
    /// <returns>The description of the tuple type constructor and an optional extra type
    /// for large tuples.</returns>
    member __.GetTupleConstructorInfo(tupleType:Type): ConstructorInfo * Type option =
        tupleConstructorInfo |> DictCache.get tupleType
