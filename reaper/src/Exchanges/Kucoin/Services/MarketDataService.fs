namespace Kucoin.Services

module MarketDataService = 
    open Serilog
    open Flurl.Http
    open System.Text.Json
    open System
    open Kucoin.Utils
    open Kucoin.Types.MarketDataApiNs
    open FSharp.Control

    let getSymbolPriceAsync (symbol: string) =
        async{

            use flurlClient = FlurlHelper.getFlurlClient (RLogger.get_default_rlogger()) true

            let request = flurlClient.Request()
                            .AppendPathSegments("api", "v1", "contracts", symbol.ToUpper())
            let signedRequest = FlurlHelper.signRequest 
                                    request 
                                    (Authorisation.readCredentials())
                                    "GET"
                                    (Some "")
            try
                let! response = signedRequest.GetStringAsync() |> Async.AwaitTask
                let symbolData = JsonSerializer.Deserialize<SymbolDetail>(response,
                        options = new JsonSerializerOptions( PropertyNameCaseInsensitive = true)).Data
                return symbolData.MarkPrice
            with
            | ex -> 
                printfn "Error: %s" ex.Message
                return 0M
        } 


    let private get_200_candles(symbol: string) (from: float) (to_: float) (intervalInMins: int) = 
        async{

            let queryParams = [
                "symbol", symbol
                "from", from.ToString()
                "to", to_.ToString()
                "granularity", intervalInMins.ToString()
            ]

            use flurlClient = FlurlHelper.getFlurlClient (RLogger.get_default_rlogger())  true

            let request = flurlClient.Request()
                            .AppendPathSegments("api", "v1", "kline", "query")
                            .SetQueryParams(queryParams)

            let signedRequest = FlurlHelper.signRequest 
                                    request 
                                    (Authorisation.readCredentials())
                                    "GET"
                                    (Some "")
            try
                let! responseStr = signedRequest.GetStringAsync() |> Async.AwaitTask
                let CandlesArray = JsonSerializer.Deserialize<KucoinCandles>(responseStr,
                        options = new JsonSerializerOptions( PropertyNameCaseInsensitive = true)).Data
                let Candles: Candle[] =
                    CandlesArray
                    |> List.map (fun x -> 
                        {
                            Time = (int64)x[0]
                            Open = x[1]
                            High = x[2]
                            Low = x[3]
                            Close = x[4]
                            Volume = x[5]
                        }) 
                        |> List.toArray
                return Candles
            with
            | ex -> 
                printfn "Error: %s" ex.Message
                return [||]
        }

    let get_candles (symbol: string) (startTime: string) (endTime: Option<string>) (interval: int) = 
        asyncSeq{
            let endTimeValue = match endTime with
                                | Some et -> et
                                | None -> DateTime.Now.ToString(TimeHelper.timeFormat)

            let mutable from = startTime |> TimeHelper.toUtcEpoch
            let to_ = endTimeValue |> TimeHelper.toUtcEpoch


            let total_num_of_candles = (to_ - from) / 1000.0 / 60.0 / float interval
            let count_of_iteration = int (ceil total_num_of_candles / 200.0)
            
            for i = 0 to count_of_iteration do
                let next_to = min (from + (200.0 * float interval) * 1000.0 * 60.0) to_
                let! iter_candles = get_200_candles symbol from next_to interval

                from <- next_to
                yield iter_candles
        }



    let getActiveContracts(): Async<Contract array> = 
        async{
            use flurlClient = FlurlHelper.getFlurlClient (RLogger.get_default_rlogger()) true

            let request = flurlClient.Request()
                            .AppendPathSegments("api", "v1", "contracts", "active")

            let signedRequest = FlurlHelper.signRequest
                                    request
                                    (Authorisation.readCredentials())
                                    "GET"
                                    (Some "")
            try
                use! responseStream = signedRequest.GetStreamAsync() |> Async.AwaitTask
                let! contracts = 
                    JsonSerializer.DeserializeAsync<ContractApiResponse>(responseStream,
                        options = new JsonSerializerOptions (PropertyNameCaseInsensitive = true)).AsTask()
                    |> Async.AwaitTask

                return contracts.Data |> List.toArray
            with
            | ex -> 
                printfn "Error: %s" ex.Message
                return [||]
        }