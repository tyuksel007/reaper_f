namespace Kucoin.Services

module OrderService =
    open Kucoin.Types
    open System.Text.Json
    open Kucoin
    open Serilog
    open Flurl.Http
    open System


    let cancelOrderAsync(symbol: string) (orderId: string): Async<unit> = 
        failwith "Not Implemented"



    let getActiveOrdersAsync(symbol: string): Async<OrderDetailsData[] option> = 
        async {
            use flurlClient = FlurlHelper.getFlurlClient Log.Logger true
            let request = flurlClient.Request()
                            .AppendPathSegments("api", "v1", "orders")
                            .SetQueryParam("symbol", symbol.ToUpper())
                            .SetQueryParam("status", "active")

            let signedRequest = FlurlHelper.signRequest 
                                    request 
                                    (Credentials.readCredentials())
                                    "GET"
                                    (Some "")
            try
                use! responseStream = signedRequest.GetStreamAsync() |> Async.AwaitTask
                let! orders = JsonSerializer.DeserializeAsync<OrdersApiResponse>(responseStream,
                                options = new JsonSerializerOptions( PropertyNameCaseInsensitive = true)).AsTask()
                                |> Async.AwaitTask

                return Some(orders.Data.Items |> Array.ofList)
            with
                | ex ->
                    printfn "Error: %s" ex.Message
                    return None
        }


    let rec waitActiveOrders(symbol: string) =
        async{
            let activeOrders = getActiveOrdersAsync(symbol) |> Async.RunSynchronously
            match activeOrders with
                | Some orders ->  if Array.length orders <> 0 then 
                                    do! Async.Sleep(5000)
                                    do! waitActiveOrders(symbol)
                | None -> ()
        }

    

    let getOrderDetailsAsync (orderId: string): Async<OrderDetailsData option> = 
        async{
            use flurlClient = FlurlHelper.getFlurlClient Log.Logger true
            let request = flurlClient.Request()
                            .AppendPathSegments("api", "v1", "orders", orderId)
            let signedRequest = FlurlHelper.signRequest
                                    request
                                    (Credentials.readCredentials())
                                    "GET"
                                    (Some "")
            try
                use! responseStream = signedRequest.GetStreamAsync() |> Async.AwaitTask
                let! orderDetailsResponse = JsonSerializer.DeserializeAsync<OrderDetailsApiResponse>(responseStream,
                                            options = new JsonSerializerOptions( PropertyNameCaseInsensitive = true)).AsTask()
                                            |> Async.AwaitTask

                return Some(orderDetailsResponse.Data)
            with
                | ex ->
                    printfn "Error: %s" ex.Message
                    return None
        }


    // wait until active orders filled
    // get order details
    // if order not filled, place order for remaining amount

    let rec placeOrderAsync (symbol: string)
                        (leverage: int)
                        (amount: decimal)
                        (side: string)
                        (orderType: string)
                        (price: decimal)
                         : Async<bool> = 
        async{

            let limitOrMarket = match orderType with
                                | "limit" -> OrderType.Limit
                                | "market" -> OrderType.Market
                                | _ -> failwith "Invalid order type"

            let limitPriceMaybe=
                        if limitOrMarket = OrderType.Market then 
                            None
                        else
                            Some price // todo encapsulate measerure must be > 0


            let buyOrSell = match side with
                            | "buy" -> Side.Buy
                            | "sell" -> Side.Sell
                            | _ -> failwith "Invalid side"

            try
                use flurlClient = FlurlHelper.getFlurlClient Log.Logger true

                let! marketPrice = MarketDataService.getSymbolPriceAsync(symbol);
                let size = amount / marketPrice

                let orderRequest : PlaceOrderRequest = { 
                        clientoid = Guid.NewGuid().ToString()
                        symbol = symbol
                        leverage = leverage
                        side = buyOrSell
                        ``type`` = limitOrMarket.ToString().ToLower()
                        size = size
                        price = limitPriceMaybe
                }

                let request = flurlClient.Request()
                                .AppendPathSegments("api", "v1", "orders")
                                .SetQueryParams(orderRequest)
                                
                let signedRequest = FlurlHelper.signRequest
                                        request
                                        (Credentials.readCredentials())
                                        "POST"
                                        (Some(JsonSerializer.Serialize(orderRequest)))

                use! responseStream = signedRequest.GetStreamAsync() |> Async.AwaitTask

                let! placeOrderRes = JsonSerializer.DeserializeAsync<PlaceOrderApiResponse>(responseStream,
                                            options = new JsonSerializerOptions( PropertyNameCaseInsensitive = true)).AsTask()
                                            |> Async.AwaitTask
                do! waitActiveOrders(symbol)

                //filling remaining amount
                let! orderDetails = getOrderDetailsAsync placeOrderRes.Data.OrderId
                let filledValue = match orderDetails with
                                    | Some order -> order.FilledValue
                                    | None -> 0m

                let remainingAmount = amount - filledValue
                let remainingAmountTreshold = 2m

                if remainingAmount > remainingAmountTreshold then
                        placeOrderAsync symbol leverage remainingAmount side orderType price  |> ignore

                return true
            with
                | ex ->
                    printfn "Error: %s" ex.Message
                    return false
        }


    


    let ensureTransactionIsComplete(orderId: string) (amount: decimal) =
        async{
            let! orderDetails = getOrderDetailsAsync orderId 
            do! waitActiveOrders(orderDetails.Value.Symbol)

            let amountToFill = amount - orderDetails.Value.FilledSize

            RLogger.appLogger.Information($"target-amount: {amount}");
            RLogger.appLogger.Information($"amount filled: {orderDetails.Value.FilledSize}");
            RLogger.appLogger.Information($"amount to fill: {amountToFill}");

            if amountToFill > 0m then
                RLogger.appLogger.Information("Filling the remaining amount")
        }

