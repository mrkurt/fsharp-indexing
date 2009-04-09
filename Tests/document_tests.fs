#light
namespace Mubble.Indexing.Tests
open System
open NUnit.Framework

open Mubble.Indexing.Units
open Mubble.Indexing

[<TestFixture>]
type DocumentTests() = 
    static member DefaultSchema
        with get () = 
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
            defaultSchema
            
    static member DefaultDocument
        with get () = 
            { 
                Schema = DocumentTests.DefaultSchema;
                Fields = 
                    [
                        ("ID", "ASDF");
                        ("PublishDate", DateTime.Now.ToString());
                        ("Title", "This is a title");
                        ("Excerpt", "My title was non-obvious");
                        ("Body", "Lorem ipsum ad infinum");
                    ] }
    
    [<Test>]
    member x.ConvertDoc () =
        let doc = DocumentTests.DefaultDocument
        let ldoc = Lucene.convert doc
        ()