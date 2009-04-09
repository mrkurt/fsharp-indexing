#light
module Mubble.Indexing.Units
open System


(* Schema Types *)
type DateResolution = Day | Hour | Millisecond | Minute | Month | Second | Year

type FieldType = String | Date of DateResolution

type FieldOption = 
    Unique | Indexed | Stored | Compressed | MultiValue 
    | Required | Tokenized | Type of FieldType

type IndexSchema = { Name : string; Version : float; Fields : Map<string, FieldOption list> }

let getSchemaField (schema : IndexSchema) field = 
        match schema.Fields.TryFind field with
        | Some(f) -> f
        | None ->
            match schema.Fields.TryFind "*" with
            | Some(f) -> f
            | None -> failwith "No matching field in schema"

type DocField = string * string
type Document = { Schema : IndexSchema; Fields : DocField list }


    (*
    let printFields (schema : IndexSchema) =
        let optionToString o = 
            match o with
            | FieldOption.Type(t) -> 
                match t with
                | Date(p) -> sprintf "Type=Date:%A" p
                | _ -> sprintf "Type=%A" t
            | _ -> sprintf "%A" o
            
        schema.Fields.Iterate (fun n f ->
            printfn "Field %s" n
            f |> List.iter (fun o -> printfn "\t%s" (optionToString o)))
            
    let defaultSchema = 
        let options = [ Indexed; Tokenized; Type(String) ]
        {   Name = "Mubble.Content";
            Version = 1.0;
            Fields = Map.of_list 
                      [ ("ID", [ Unique; Indexed; Stored; Required; ] );
                        ( "PublishDate", [ Indexed; Type(Date Minute) ] );
                        ( "Title", options );
                        ( "Excerpt", options );
                        ( "Body", MultiValue :: options ) ] 
        }  

    let sampleDoc = { 
        Schema = defaultSchema;
        Fields = 
            [
                ("ID", "ASDF");
                ("PublishDate", DateTime.Now.ToString());
                ("Title", "This is a title");
                ("Excerpt", "My title was non-obvious");
                ("Body", "Lorem ipsum ad infinum");
            ] }*)