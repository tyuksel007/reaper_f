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


    [<HttpGet("klines")>]
    member _.GetKlines(symbol: string, startTime: string, endTime: string, interval: int) =
        async {
            let! klines = MarketDataService.getKlines symbol startTime (Some endTime) interval
            return klines
        }

    
    [<HttpGet("contracts")>]
    member _.GetContracts() =
        async {
            let! contracts = MarketDataService.getActiveContracts()
            return contracts
        }