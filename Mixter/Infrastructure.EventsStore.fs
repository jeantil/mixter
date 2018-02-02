﻿module Mixter.Infrastructure.EventsStore

open System
open System.Collections.Generic

type MemoryEventsStore () = 
    let eventsByAggregate = Dictionary<Object, Object list>();

    member _.Store (aggregateId: 'a) (events: 'b list) : unit = 
        if not (eventsByAggregate.ContainsKey(aggregateId))
        then eventsByAggregate.Add(aggregateId, [])
        
        let currentEvents = eventsByAggregate[aggregateId]
        let newEvents = currentEvents @ (events |> List.map (fun c -> c :> Object))

        eventsByAggregate[aggregateId] <- newEvents
    
    member _.Get<'b> aggregateId : 'b list = 
        match eventsByAggregate.ContainsKey(aggregateId) with
        | false -> []
        | true -> eventsByAggregate[aggregateId] |> List.map (fun c -> c :?> 'b)