#light
module Mubble.Indexing.Concurrency

open System
#nowarn "57"
open Microsoft.FSharp.Control.Mailboxes
open Microsoft.FSharp.Control.SharedMemory.Helpers
open Lucene.Net
open System.Threading

type SearcherManager(initial : Search.IndexSearcher) = 
    let searcher = ref initial
    let lock = new ReaderWriterLock()
    
    member x.Instance 
        with set v =
            writeLock lock (fun () ->
                                let old = !searcher
                                searcher := v
                                old.Close())
                
    member x.Query(f) = 
        readLock lock (fun () -> f !searcher)

type IndexManager(path : string) = 
    let searcher = new SearcherManager(new Search.IndexSearcher(path))
    let lastOpened = ref (DateTime.Now)
    
    let searchop f : (Search.IndexSearcher -> 'a)  =
        searcher.Query f
            
    let resetSearcher () = 
        let requestedAt = DateTime.Now
        lock searcher (fun () ->
                            if requestedAt > !lastOpened then
                                lastOpened := DateTime.Now
                                let newSearcher = new Search.IndexSearcher(path)
                                searcher.Instance <- newSearcher
                            else
                                ())
                                
    let writeop (inbox : (Index.IndexWriter -> DateTime -> unit) MailboxProcessor) =
        let rec loop (last : DateTime) =
            async{  let! msg = inbox.Receive()
                    let writer = new Index.IndexWriter(path, Analysis.Standard.StandardAnalyzer(), true)
                    try
                        do msg writer last
                        do resetSearcher ()
                    finally
                        do writer.Close()
                        ()
                    return! loop (DateTime.Now) }
        loop (DateTime.MinValue)
                
    let writemb = MailboxProcessor.Start(writeop);  
    
    member x.Write(f) = writemb.Post(f)
    member x.Search(f) = searchop f
    