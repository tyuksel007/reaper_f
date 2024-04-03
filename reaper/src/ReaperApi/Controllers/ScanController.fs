namespace ReaperApi.Controllers
open Microsoft.AspNetCore.Mvc

[<ApiController>]
[<Route("[controller]")>]
type ScanController () =
    inherit ControllerBase()

    [<HttpGet("scan")>]
    member _.Scan(exchange: string) =
        match exchange with
            | "kucoin" -> 
                async {
                    // let symbols = Kucoin.Services.MarketDataService.get_symbols()
                    //
                    return "Kucoin"
                }
            | "binance" ->
                async {
                    return "Binance"
                }
            | _ -> async{
                return "exchange not supported"
            }
        
