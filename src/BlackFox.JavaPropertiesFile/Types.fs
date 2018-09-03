namespace BlackFox.JavaPropertiesFile

type Entry =
    | Comment of text : string
    | KeyValue of key : string * value : string
