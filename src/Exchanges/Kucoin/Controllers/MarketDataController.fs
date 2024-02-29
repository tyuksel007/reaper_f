namespace Kucoin.Controllers

open Microsoft.AspNetCore.Mvc
open Kucoin.Services

[<ApiController>]
[<Route("[controller]")>]
type MarketDataController() =
    inherit ControllerBase()

    [<HttpGet("symbolprice")>]
    member _.GetSymbolPrice(symbol: string) =
        async {
            let! price = MarketDataService.getSymbolPriceAsync(symbol) 
            return price
        }


    [<HttpGet("Candles")>]
    member _.GetCandles(symbol: string, startTime: string, endTime: string, interval: int) =
        async {
            let! candles = MarketDataService.getCandles symbol startTime (Some endTime) interval
            return candles
        }

    
    [<HttpGet("contracts")>]
    member _.GetContracts() =
        async {
            let! contracts = MarketDataService.getActiveContracts()
            return contracts
        }