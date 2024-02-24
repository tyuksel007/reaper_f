namespace Kucoin

module Scan =
    open Kucoin.Services
    open System

    let calculateScore (symbol: string) = 
        // get klines
        //tilson_score: 
        // bollinger_band_score:
        // macd_score:
        async{
            let startTime = DateTime.UtcNow.AddHours(-8).ToString(TimeHelper.timeFormat)
            let! klines = MarketDataService.getKlines symbol startTime None 5
            return 0.0
        }

        
    let getSymbolToTrade = 
        async {
            let! contracts =  MarketDataService.getActiveContracts()
            let! scores = 
                contracts
                |> Array.map (fun c ->
                    async{
                        let! score = calculateScore c.Symbol
                        return (c.Symbol, score)
                    }
                ) 
                |> Async.Parallel
            let sortedScores = scores |> Array.sortByDescending (fun (_, score) -> score)
            let (symbol, _) = sortedScores[0]
            return symbol
        }
