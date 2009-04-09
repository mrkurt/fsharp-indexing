#light
module Mubble.Indexing.Lucene
open System
open Mubble.Indexing
open Units

open Lucene.Net

type Lucene.Net.Index.IndexWriter with
    member x.AddSchema (schema:IndexSchema) =
        let dir = x.GetDirectory()
        ()

type DateTools = Documents.DateTools
let convertResolution r =
    match r with
    | Day -> DateTools.Resolution.DAY
    | Hour -> DateTools.Resolution.HOUR
    | Millisecond -> DateTools.Resolution.MILLISECOND
    | Minute -> DateTools.Resolution.MINUTE
    | Month -> DateTools.Resolution.MONTH
    | Second -> DateTools.Resolution.SECOND
    | Year -> DateTools.Resolution.YEAR

let format (t : FieldType) raw =
    match t with
    | FieldType.Date(r) -> 
        let d = DateTime.Parse(raw)
        DateTools.DateToString(d, (convertResolution r))
    | String -> raw

let buildField (schema : IndexSchema) (field : DocField) = 
    let name = ref (fst field)
    let value = ref (snd field)
    let store = ref Documents.Field.Store.NO
    let index = ref Documents.Field.Index.TOKENIZED
    
    let options = getSchemaField schema !name
       
    let setOption o =
        match o with
        | FieldOption.Stored -> store := Documents.Field.Store.YES
        | FieldOption.Compressed -> store := Documents.Field.Store.COMPRESS
        | FieldOption.Indexed -> index := Documents.Field.Index.UN_TOKENIZED
        | FieldOption.Tokenized -> index := Documents.Field.Index.TOKENIZED
        | FieldOption.Type(t) -> value := format t !value
        // The following aren't relevant here
        | FieldOption.Required | FieldOption.Unique
        | FieldOption.MultiValue -> ()

    options |> List.iter setOption
    
    let lfield = new Documents.Field(!name, !value, !store, !index)
    lfield

let convert (doc : Document) = 
    let ldoc = new Documents.Document()
    
    let lFields = doc.Fields |> List.map (buildField doc.Schema)
    
    lFields |> List.iter ldoc.Add    
    ldoc

type WriteOp = Index of Document | Batch of WriteOp list | Schema of IndexSchema | Optimize  

let rec write (index : Index.IndexWriter) op = 
    let write' doc = 
        let d' = convert doc
        index.AddDocument(d')
    match op with
    | Index(doc) -> write' doc
    | Schema(s) -> printfn "Saving schema: %s" s.Name
    | Optimize -> index.Optimize()
    | Batch(h::t) -> 
        write index h
        write index (Batch t)  
    | Batch([]) -> ()
    
(*let doc () : Document = 
    { 
        Schema = defaultSchema; 
        Fields = 
            [
                ( "ID", Guid.NewGuid().ToString() );
                ( "Title", "Some Content" );
                ( "PublishDate", DateTime.Now.ToString() );
                ( "Excerpt", "Some excerpt" );
                ( "Body", "A body of text!" )
            ]
    }*)