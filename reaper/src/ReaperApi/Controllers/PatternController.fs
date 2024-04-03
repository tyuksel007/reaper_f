namespace ReaperApi.Controllers
open Microsoft.AspNetCore.Mvc

[<ApiController>]
[<Route("[controller]")>]
type PatternController () =
    inherit ControllerBase()

    [<HttpGet("pattern")>]
    member _.GetChartData(symbol: string) (pattern: string) =
        match pattern with
            | "broadening_bottoms" -> 
                async {
                    return "Kucoin"
                }
            | _ -> async{
                return "pattern not supported"
            }
        
