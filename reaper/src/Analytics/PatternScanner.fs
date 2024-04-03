namespace Analytics

module PatternScanner =
    open Analytics.Types.MarketNs
    open Analytics.Types.AnalysisNs
    open System
    open Analytics.Types.TradeNs
    open FSharp.Control



    let private save_pattern_order (scanResult: PatternOrder) = 
        async {
            failwith "not implemented"
        }


    //to be implemented
    let private signal_via_telegram (order: PatternOrder) =
        async {
            match order.Signal with
                | nameof SignalType.Buy | nameof SignalType.Sell -> 
                    ()
                | _ -> ()
        }


    let private there_is_a_signal (pattern: PatternOrder) =
            if pattern.Signal <> nameof SignalType.SignalUndefined then
                true
            else
                false


    let do_pattern_analysis (contract: Contract) (candles: Candle array) (interval: int) =
        async {
            do Database.CandleOps.save_candles 
                contract.Symbol
                interval
                candles
                
            [|
                Patterns.Broadening_Bottoms.run_analysis
                Patterns.Broadening_Tops.run_analysis
            |]
                |> Array.map (fun pattern_fn -> async {
                    do! pattern_fn contract interval
                        |> AsyncSeq.iterAsync (fun orders -> 
                            async {
                                for order in orders  do
                                    do! save_pattern_order order
                                    do! signal_via_telegram order
                            } 
                        ) 
                })
                |> Async.Parallel 
                |> ignore
        }


