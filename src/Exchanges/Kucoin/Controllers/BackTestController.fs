namespace Kucoin.Controllers

open Microsoft.AspNetCore.Mvc
open Kucoin.Services

[<ApiController>]
[<Route("[controller]")>]
type BackTestController() =
    inherit ControllerBase()

    [<HttpGet("backtest")>]
    member _.BackTest(symbol: string, startTime: string, endTime: string, interval: int, capital: decimal) =
        async {
            let! candles = MarketDataService.getCandles symbol startTime None interval
            return Analytics.Patterns.Broadening_Bottoms.save (symbol.ToLower()) candles
        }