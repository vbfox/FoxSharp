namespace BlackFox.CachedFSharpReflection

open System.Collections.Concurrent

type private DictCache<'key, 'dictKey, 'value> = {
    Dict : ConcurrentDictionary<'dictKey, 'value>
    KeyConverter: 'key -> 'dictKey
    Getter: 'key -> 'value
}

module private DictCache =
    let create keyConverter getter =
        {
            Dict = ConcurrentDictionary<_,_>()
            KeyConverter = keyConverter
            Getter = getter
        }

    let get key cache =
        let dictKey = cache.KeyConverter key
        cache.Dict.GetOrAdd(dictKey, fun _ -> cache.Getter key)

    let clear cache =
        cache.Dict.Clear()
