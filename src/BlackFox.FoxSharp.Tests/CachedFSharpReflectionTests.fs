module BlackFox.FoxSharp.Tests.CachedFSharpReflectionTests

open System
open System.IO
open System.Reflection
open System.Threading.Tasks
open Microsoft.FSharp.Reflection
open Expecto
open Expecto.Flip
open BlackFox.CachedFSharpReflection

type TestRecord = { Name: string; Age: int }

/// Shares a field name with TestRecord to check that field readers aren't mixed up between record types
type OtherRecord = { Name: string; Enabled: bool }

type GenericRecord<'T> = { Value: 'T }

type TestUnion =
    | Empty
    | One of int
    | Two of string * float

/// Shares a case name with TestUnion to check that case readers aren't mixed up between union types
type OtherUnion =
    | One of string
    | Three

exception TestException of Code: int * Message: string

module TestModule =
    type Marker = class end

let private moduleType = typeof<TestModule.Marker>.DeclaringType

let private record: TestRecord = { Name = "Alice"; Age = 42 }
let private otherRecord: OtherRecord = { Name = "Bob"; Enabled = true }
let private unionValues = [ TestUnion.Empty; TestUnion.One 1; TestUnion.Two("two", 2.0) ]
let private smallTuple = (1, "two", 3.0)
let private bigTuple = (1, 2, 3, 4, 5, 6, 7, 8, 9, 10)

let private sampleTypes = [
    typeof<TestRecord>; typeof<OtherRecord>; typeof<GenericRecord<int>>; typedefof<GenericRecord<_>>
    typeof<TestUnion>; typeof<OtherUnion>; typeof<int list>; typeof<int option>
    typeof<TestException>; typeof<exn>
    smallTuple.GetType(); bigTuple.GetType(); typeof<struct (int * string)>
    typeof<int -> string>; typeof<int -> string -> float>
    moduleType
    typeof<int>; typeof<string>; typeof<obj>; typeof<int[]>; typeof<Guid>
]

let private typePredicates: (string * (FSharpTypeCache -> Type -> bool) * (Type -> bool)) list = [
    "IsFunction", (fun c t -> c.IsFunction t), (fun t -> FSharpType.IsFunction t)
    "IsModule", (fun c t -> c.IsModule t), (fun t -> FSharpType.IsModule t)
    "IsTuple", (fun c t -> c.IsTuple t), (fun t -> FSharpType.IsTuple t)
    "IsRecord", (fun c t -> c.IsRecord t), (fun t -> FSharpType.IsRecord t)
    "IsUnion", (fun c t -> c.IsUnion t), (fun t -> FSharpType.IsUnion t)
    "IsExceptionRepresentation", (fun c t -> c.IsExceptionRepresentation t), (fun t -> FSharpType.IsExceptionRepresentation t)
]

/// Loads a second copy of the test assembly from its bytes.
/// Types from that copy have the same FullName as the originals but are distinct Type instances,
/// which is what a cache keyed on FullName would get wrong.
let private loadSecondCopyOfTestAssembly () =
    Assembly.Load(File.ReadAllBytes typeof<TestRecord>.Assembly.Location)

let private secondCopy = lazy (loadSecondCopyOfTestAssembly ())

let private typeFromSecondCopy (t: Type) =
    let copyType = secondCopy.Value.GetType(t.FullName, true)
    Expect.equal "Both types must have the same FullName" t.FullName copyType.FullName
    Expect.notEqual "The types must be distinct instances" t copyType
    copyType

let private unionCase (unionType: Type) (name: string) =
    FSharpType.GetUnionCases unionType |> Array.find (fun c -> c.Name = name)

let private typeCacheTests =
    testList "FSharpTypeCache" [
        testList "predicates match FSharpType" [
            for name, cached, direct in typePredicates do
                for t in sampleTypes do
                    yield testCase (sprintf "%s(%O)" name t) <| fun () ->
                        let cache = FSharpTypeCache()
                        Expect.equal "Cached result must match FSharpType" (direct t) (cached cache t)
        ]

        testCase "GetTupleElements matches FSharpType" <| fun () ->
            let cache = FSharpTypeCache()
            for t in [ smallTuple.GetType(); bigTuple.GetType(); typeof<struct (int * string)> ] do
                Expect.equal (string t) (FSharpType.GetTupleElements t) (cache.GetTupleElements t)

        testCase "GetFunctionElements matches FSharpType" <| fun () ->
            let cache = FSharpTypeCache()
            for t in [ typeof<int -> string>; typeof<int -> string -> float> ] do
                Expect.equal (string t) (FSharpType.GetFunctionElements t) (cache.GetFunctionElements t)

        testCase "GetRecordFields matches FSharpType" <| fun () ->
            let cache = FSharpTypeCache()
            for t in [ typeof<TestRecord>; typeof<OtherRecord>; typeof<GenericRecord<int>> ] do
                Expect.equal (string t) (FSharpType.GetRecordFields t) (cache.GetRecordFields t)

        testCase "GetUnionCases matches FSharpType" <| fun () ->
            let cache = FSharpTypeCache()
            for t in [ typeof<TestUnion>; typeof<OtherUnion>; typeof<int option> ] do
                Expect.equal (string t) (FSharpType.GetUnionCases t) (cache.GetUnionCases t)

        testCase "GetExceptionFields matches FSharpType" <| fun () ->
            let cache = FSharpTypeCache()
            let t = typeof<TestException>
            Expect.equal (string t) (FSharpType.GetExceptionFields t) (cache.GetExceptionFields t)

        testCase "GetRecordFields returns the cached instance until Clear" <| fun () ->
            let cache = FSharpTypeCache()
            let first = cache.GetRecordFields typeof<TestRecord>
            let second = cache.GetRecordFields typeof<TestRecord>
            Expect.isTrue "The second call must return the cached array" (obj.ReferenceEquals(first, second))
            cache.Clear()
            let third = cache.GetRecordFields typeof<TestRecord>
            Expect.isFalse "After Clear the value must be computed again" (obj.ReferenceEquals(first, third))
            Expect.equal "The recomputed value must be the same" first third

        testCase "Concurrent calls all get the same cached instance" <| fun () ->
            let cache = FSharpTypeCache()
            let results = Array.zeroCreate<UnionCaseInfo[]> 1000
            Parallel.For(0, results.Length, fun i -> results.[i] <- cache.GetUnionCases typeof<TestUnion>) |> ignore
            Expect.all "Every call must return the single cached instance" (fun r -> obj.ReferenceEquals(r, results.[0])) results

        testCase "GetRecordFields on a non-record throws ArgumentException every time" <| fun () ->
            let cache = FSharpTypeCache()
            Expect.throwsT<ArgumentException> "First call" (fun () -> cache.GetRecordFields typeof<int> |> ignore)
            Expect.throwsT<ArgumentException> "Second call" (fun () -> cache.GetRecordFields typeof<int> |> ignore)

        testCase "Types with the same FullName from different assemblies are cached separately" <| fun () ->
            let cache = FSharpTypeCache()
            let original = typeof<TestRecord>
            let copy = typeFromSecondCopy original
            Expect.isTrue "The original is a record" (cache.IsRecord original)
            Expect.isTrue "The copy is a record" (cache.IsRecord copy)
            Expect.all "Fields of the original belong to the original" (fun (p: PropertyInfo) -> p.DeclaringType = original) (cache.GetRecordFields original)
            Expect.all "Fields of the copy belong to the copy" (fun (p: PropertyInfo) -> p.DeclaringType = copy) (cache.GetRecordFields copy)
            let unionCopy = typeFromSecondCopy typeof<TestUnion>
            Expect.all "Cases of the copy belong to the copy" (fun (c: UnionCaseInfo) -> c.DeclaringType = unionCopy) (cache.GetUnionCases unionCopy)
    ]

let private valueCacheTests =
    testList "FSharpValueCache" [
        testCase "GetRecordReader reads the fields in declaration order" <| fun () ->
            let cache = FSharpValueCache()
            let reader = cache.GetRecordReader typeof<TestRecord>
            Expect.equal "Fields" [| box "Alice"; box 42 |] (reader record)
            Expect.equal "Must match FSharpValue" (FSharpValue.GetRecordFields record) (reader record)

        testCase "GetRecordConstructor builds a record" <| fun () ->
            let cache = FSharpValueCache()
            let ctor = cache.GetRecordConstructor typeof<TestRecord>
            Expect.equal "Constructed record" record (unbox<TestRecord> (ctor [| box "Alice"; box 42 |]))

        testCase "GetRecordConstructorInfo matches FSharpValue" <| fun () ->
            let cache = FSharpValueCache()
            let t = typeof<TestRecord>
            Expect.equal "ConstructorInfo" (FSharpValue.PreComputeRecordConstructorInfo t) (cache.GetRecordConstructorInfo t)

        testCase "GetRecordFieldReader reads each field" <| fun () ->
            let cache = FSharpValueCache()
            for p in FSharpType.GetRecordFields typeof<TestRecord> do
                let reader = cache.GetRecordFieldReader p
                Expect.equal p.Name (p.GetValue record) (reader record)

        testCase "GetRecordFieldReader distinguishes fields with the same name on different records" <| fun () ->
            let cache = FSharpValueCache()
            let readTestName = cache.GetRecordFieldReader (typeof<TestRecord>.GetProperty "Name")
            let readOtherName = cache.GetRecordFieldReader (typeof<OtherRecord>.GetProperty "Name")
            Expect.equal "TestRecord.Name" (box "Alice") (readTestName record)
            Expect.equal "OtherRecord.Name" (box "Bob") (readOtherName otherRecord)

        testCase "GetRecordFieldReader distinguishes generic instantiations" <| fun () ->
            let cache = FSharpValueCache()
            let readInt = cache.GetRecordFieldReader (typeof<GenericRecord<int>>.GetProperty "Value")
            let readString = cache.GetRecordFieldReader (typeof<GenericRecord<string>>.GetProperty "Value")
            Expect.equal "GenericRecord<int>.Value" (box 1) (readInt { Value = 1 })
            Expect.equal "GenericRecord<string>.Value" (box "s") (readString { Value = "s" })

        testCase "GetUnionTagReader matches FSharpValue.GetUnionFields" <| fun () ->
            let cache = FSharpValueCache()
            let tagReader = cache.GetUnionTagReader typeof<TestUnion>
            for v in unionValues do
                let case, _ = FSharpValue.GetUnionFields(v, typeof<TestUnion>)
                Expect.equal (sprintf "%A" v) case.Tag (tagReader v)

        testCase "GetUnionTagMemberInfo matches FSharpValue" <| fun () ->
            let cache = FSharpValueCache()
            let t = typeof<TestUnion>
            Expect.equal "MemberInfo" (FSharpValue.PreComputeUnionTagMemberInfo t) (cache.GetUnionTagMemberInfo t)

        testCase "GetUnionReader and GetUnionConstructor round-trip every case" <| fun () ->
            let cache = FSharpValueCache()
            for v in unionValues do
                let case, expectedFields = FSharpValue.GetUnionFields(v, typeof<TestUnion>)
                let fields = cache.GetUnionReader case v
                Expect.equal (sprintf "Fields of %A" v) expectedFields fields
                Expect.equal (sprintf "Rebuilt %A" v) v (unbox<TestUnion> (cache.GetUnionConstructor case fields))

        testCase "GetUnionConstructorInfo matches FSharpValue" <| fun () ->
            let cache = FSharpValueCache()
            for case in FSharpType.GetUnionCases typeof<TestUnion> do
                Expect.equal case.Name (FSharpValue.PreComputeUnionConstructorInfo case) (cache.GetUnionConstructorInfo case)

        testCase "Union cases with the same name on different unions are cached separately" <| fun () ->
            let cache = FSharpValueCache()
            let buildTestOne = cache.GetUnionConstructor (unionCase typeof<TestUnion> "One")
            let buildOtherOne = cache.GetUnionConstructor (unionCase typeof<OtherUnion> "One")
            Expect.equal "TestUnion.One" (TestUnion.One 1) (unbox<TestUnion> (buildTestOne [| box 1 |]))
            Expect.equal "OtherUnion.One" (OtherUnion.One "x") (unbox<OtherUnion> (buildOtherOne [| box "x" |]))

        testCase "GetTupleReader and GetTupleConstructor round-trip small and big tuples" <| fun () ->
            let cache = FSharpValueCache()
            for (name, value) in [ "small", box smallTuple; "big", box bigTuple ] do
                let t = value.GetType()
                let fields = cache.GetTupleReader t value
                Expect.equal (name + " fields") (FSharpValue.GetTupleFields value) fields
                Expect.equal (name + " rebuilt") value (cache.GetTupleConstructor t fields)

        testCase "GetTuplePropertyInfo matches FSharpValue for every index of a big tuple" <| fun () ->
            let cache = FSharpValueCache()
            let t = bigTuple.GetType()
            for i in 0 .. FSharpType.GetTupleElements(t).Length - 1 do
                Expect.equal (string i) (FSharpValue.PreComputeTuplePropertyInfo(t, i)) (cache.GetTuplePropertyInfo(t, i))

        testCase "GetTupleConstructorInfo matches FSharpValue" <| fun () ->
            let cache = FSharpValueCache()
            for t in [ smallTuple.GetType(); bigTuple.GetType() ] do
                Expect.equal (string t) (FSharpValue.PreComputeTupleConstructorInfo t) (cache.GetTupleConstructorInfo t)
            Expect.isSome "A big tuple has a nested type" (snd (cache.GetTupleConstructorInfo (bigTuple.GetType())))

        testCase "GetRecordReader returns the cached function until Clear" <| fun () ->
            let cache = FSharpValueCache()
            let first = cache.GetRecordReader typeof<TestRecord>
            let second = cache.GetRecordReader typeof<TestRecord>
            Expect.isTrue "The second call must return the cached function" (obj.ReferenceEquals(first, second))
            cache.Clear()
            let third = cache.GetRecordReader typeof<TestRecord>
            Expect.isFalse "After Clear the function must be computed again" (obj.ReferenceEquals(first, third))

        testCase "GetRecordReader on a non-record throws ArgumentException" <| fun () ->
            let cache = FSharpValueCache()
            Expect.throwsT<ArgumentException> "Non-record" (fun () -> cache.GetRecordReader typeof<int> |> ignore)

        testCase "Types with the same FullName from different assemblies are cached separately" <| fun () ->
            let cache = FSharpValueCache()
            let original = typeof<TestRecord>
            let copy = typeFromSecondCopy original
            let fields = [| box "Alice"; box 42 |]
            Expect.equal "Original constructor builds the original type" original ((cache.GetRecordConstructor original fields).GetType())
            Expect.equal "Copy constructor builds the copy type" copy ((cache.GetRecordConstructor copy fields).GetType())
            let copyValue = cache.GetRecordConstructor copy fields
            Expect.equal "Copy reader reads the copy value" fields (cache.GetRecordReader copy copyValue)
            let copyCase = unionCase (typeFromSecondCopy typeof<TestUnion>) "One"
            let copyUnionValue = cache.GetUnionConstructor copyCase [| box 1 |]
            Expect.isTrue "Copy union constructor builds the copy type" (copyCase.DeclaringType.IsAssignableFrom(copyUnionValue.GetType()))
            Expect.isFalse "The copy union value isn't an instance of the original type" (typeof<TestUnion>.IsAssignableFrom(copyUnionValue.GetType()))
    ]

let private sharedTests =
    testList "Shared" [
        testCase "Shared caches are singletons" <| fun () ->
            Expect.isTrue "FSharpTypeCache.Shared" (obj.ReferenceEquals(FSharpTypeCache.Shared, FSharpTypeCache.Shared))
            Expect.isTrue "FSharpValueCache.Shared" (obj.ReferenceEquals(FSharpValueCache.Shared, FSharpValueCache.Shared))
            Expect.isTrue "FSharpReflectionCache.Shared" (obj.ReferenceEquals(FSharpReflectionCache.Shared, FSharpReflectionCache.Shared))

        testCase "FSharpReflectionCache.Shared exposes the shared type and value caches" <| fun () ->
            Expect.isTrue "FSharpType" (obj.ReferenceEquals(FSharpReflectionCache.Shared.FSharpType, FSharpTypeCache.Shared))
            Expect.isTrue "FSharpValue" (obj.ReferenceEquals(FSharpReflectionCache.Shared.FSharpValue, FSharpValueCache.Shared))

        testCase "A new FSharpReflectionCache has its own caches" <| fun () ->
            let cache = FSharpReflectionCache()
            Expect.isFalse "FSharpType" (obj.ReferenceEquals(cache.FSharpType, FSharpTypeCache.Shared))
            Expect.isFalse "FSharpValue" (obj.ReferenceEquals(cache.FSharpValue, FSharpValueCache.Shared))

        testCase "FSharpReflectionCache wraps the caches it is given" <| fun () ->
            let typeCache = FSharpTypeCache()
            let valueCache = FSharpValueCache()
            let cache = FSharpReflectionCache(valueCache, typeCache)
            Expect.isTrue "FSharpType" (obj.ReferenceEquals(cache.FSharpType, typeCache))
            Expect.isTrue "FSharpValue" (obj.ReferenceEquals(cache.FSharpValue, valueCache))
    ]

[<Tests>]
let tests =
    testList "CachedFSharpReflection" [
        typeCacheTests
        valueCacheTests
        sharedTests
    ]
