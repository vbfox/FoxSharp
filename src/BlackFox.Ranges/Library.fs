namespace BlackFox.Ranges

open System
open System.Collections.Generic

[<Flags>]
type IntervalEndpoints =
    | Open = 0
    | RightClosed = 1
    | LeftClosed = 2
    | Closed = 3

[<Struct>]
type RangeData<'t> =
    | Empty
    | Scalar of scalarValue : 't
    | Continuous of continuousMin: 't * continuousMax: 't * endpoints: IntervalEndpoints
    | Dicrete of discreteValues : 't []
    | Composite of compositeRanges : RangeData<'t> []

type Range<'t> = {
    Data: RangeData<'t>
    Comparer: IComparer<'t>
    Min: ValueOption<'t>
    Max: ValueOption<'t>
}

module Range =
    // Get the minimum value of the range
    let tryMin (r: Range<'t>): ValueOption<'t> =
        r.Min

    /// Get the minimum value of the range or throw if empty
    let min (r: Range<'t>): 't =
        match r.Min with
        | ValueSome x -> x
        | ValueNone -> raise (InvalidOperationException "The range is empty")

    // Get the maximum value of the range
    let tryMax (r: Range<'t>): ValueOption<'t> =
        r.Max

    /// Get the maximum value of the range or throw if empty
    let max (r: Range<'t>): 't =
        match r.Max with
        | ValueSome x -> x
        | ValueNone -> raise (InvalidOperationException "The range is empty")

    let isEmpty (r: Range<'t>): bool =
        match r.Data with
        | Empty -> true
        | _ -> false

    let isScalar (r: Range<'t>): bool =
        match r.Data with
        | Scalar _ -> true
        | _ -> false

    let rec private isCountableData (r: RangeData<'t>) (comparer: IComparer<'t>): bool =
        match r with
        | Empty
        | Scalar _
        | Dicrete _ -> true
        | Continuous (min, max, _) ->
            comparer.Compare(max, min) <= 0
        | Composite comp ->
            let mutable i = 0
            let mutable countable = true
            let count = comp.Length
            while i < count && countable do
                let current = comp.[i]
                countable <- isCountableData current comparer
                i <- i + 1
            countable

    let isCountable (r: Range<'t>): bool =
        isCountableData r.Data r.Comparer
