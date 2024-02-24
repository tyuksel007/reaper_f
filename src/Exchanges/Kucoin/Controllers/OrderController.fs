namespace Kucoin.Controllers

open Microsoft.AspNetCore.Mvc
open Kucoin.Services
open Kucoin.Types

[<ApiController>]
[<Route("[controller]")>]
type OrderController() =
    inherit ControllerBase()

    [<HttpGet("placeOrder")>]
    member _.PlaceOrder(symbol: string,
             side: string,
             orderType: string,
             leverage: int,
             price: decimal,
             amount: decimal) =
        async {

            let! order = OrderService.placeOrderAsync symbol
                                    leverage
                                    amount 
                                    side
                                    orderType
                                    price 

            return order
        }

    