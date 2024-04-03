namespace Analytics

//stochastic volatility model
module SVM = 
    open Analytics.Types.MarketNs
    open System

    let computeLogReturns (data: Candle array) : decimal array =
        data 
        |> Array.sortBy (fun x -> x.Time)
        |> Array.map (fun x -> x.Close)
        |> fun closes -> Array.zip (closes) (Array.tail closes)
        |> Array.map (fun (prev, current) -> decimal (Math.Log(float current / float prev)))

