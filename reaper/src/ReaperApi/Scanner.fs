namespace Reaper

module Scanner =
    let scan (exchange: string) =
        async{
            match exchange with
            | "kucoin" ->  ()
            | "binance" -> ()
            | "mexc" -> ()
            | _ -> printfn "Exchange not supported"
        }
