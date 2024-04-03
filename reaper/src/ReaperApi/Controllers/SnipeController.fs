namespace ReaperApi.Controllers
open Microsoft.AspNetCore.Mvc

[<ApiController>]
[<Route("api/[controller]")>]
type SnipeController () =
    inherit Controller()

    member _.Post(symbol: string, amount: decimal, exchange: string) =
        ()

    