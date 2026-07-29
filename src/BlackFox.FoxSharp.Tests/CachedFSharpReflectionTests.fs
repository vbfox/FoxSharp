module BlackFox.FoxSharp.Tests.CachedFSharpReflectionTests

open Expecto
open Expecto.Flip
open BlackFox.CachedFSharpReflection

type SomeRecord = { Value: int }
type SomeUnion = | First | Second of int
type SomeGeneric<'T> = { Generic: 'T }

[<Tests>]
let typeCache =
    testList "FSharpTypeCache" [
        testCase "Records and unions are recognized" <| fun () ->
            let cache = FSharpTypeCache()
            Expect.equal "record" true (cache.IsRecord typeof<SomeRecord>)
            Expect.equal "record isn't a union" false (cache.IsUnion typeof<SomeRecord>)
            Expect.equal "union" true (cache.IsUnion typeof<SomeUnion>)
            Expect.equal "union isn't a record" false (cache.IsRecord typeof<SomeUnion>)

        // `Type.FullName` is `null` for a generic parameter. It was used as the cache key, which made every call
        // throw `ArgumentNullException`.
        testCase "Generic parameters can be queried" <| fun () ->
            let genericParameter = typedefof<SomeGeneric<_>>.GetGenericArguments().[0]
            let cache = FSharpTypeCache()
            Expect.equal "record" false (cache.IsRecord genericParameter)
            Expect.equal "union" false (cache.IsUnion genericParameter)

        testCase "A cached answer is the same as the first one" <| fun () ->
            let cache = FSharpTypeCache()
            let names () = cache.GetRecordFields typeof<SomeRecord> |> Array.map (fun f -> f.Name)
            Expect.equal "fields" [| "Value" |] (names ())
            Expect.equal "fields again" [| "Value" |] (names ())
    ]

[<Tests>]
let valueCache =
    testList "FSharpValueCache" [
        testCase "Record reader reads the fields of the record it was asked for" <| fun () ->
            let cache = FSharpValueCache()
            let reader = cache.GetRecordReader typeof<SomeRecord>
            Expect.equal "values" [| box 42 |] (reader (box { Value = 42 }))

        testCase "Record constructor builds the record it was asked for" <| fun () ->
            let cache = FSharpValueCache()
            let ctor = cache.GetRecordConstructor typeof<SomeRecord>
            Expect.equal "record" (box { Value = 7 }) (ctor [| box 7 |])

        testCase "Union tag reader tells the cases apart" <| fun () ->
            let cache = FSharpValueCache()
            let tagReader = cache.GetUnionTagReader typeof<SomeUnion>
            Expect.equal "First" 0 (tagReader (box First))
            Expect.equal "Second" 1 (tagReader (box (Second 1)))
    ]
