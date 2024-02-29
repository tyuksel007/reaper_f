namespace Kucoin.Services
open Serilog
open Kucoin
open Flurl.Http
open System.Text.Json
open Kucoin.Types
open System

module MarketDataService = 
        open Analytics.Types

        let getSymbolPriceAsync (symbol: string) =
            async{

                use flurlClient = FlurlHelper.getFlurlClient Log.Logger true

                let request = flurlClient.Request()
                                .AppendPathSegments("api", "v1", "contracts", symbol.ToUpper())
                let signedRequest = FlurlHelper.signRequest 
                                        request 
                                        (Credentials.readCredentials())
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



        let getCandles(symbol: string) (startTime: string) (endTime: Option<string>) (interval: int) = 
            async{
                
                let endTimeValue = match endTime with
                                    | Some et -> et
                                    | None -> DateTime.Now.ToString(TimeHelper.timeFormat)

                let from = startTime |> TimeHelper.toUtcEpoch
                let endOf = endTimeValue |> TimeHelper.toUtcEpoch

                let queryParams = [
                    "symbol", symbol
                    "from", from.ToString()
                    "to", endOf.ToString()
                    "granularity", interval.ToString()
                ]

                use flurlClient = FlurlHelper.getFlurlClient Log.Logger true

                let request = flurlClient.Request()
                                .AppendPathSegments("api", "v1", "kline", "query")
                                .SetQueryParams(queryParams)

                let signedRequest = FlurlHelper.signRequest 
                                        request 
                                        (Credentials.readCredentials())
                                        "GET"
                                        (Some "")
                try
                    let! responseStr = signedRequest.GetStringAsync() |> Async.AwaitTask
                    let CandlesArray = JsonSerializer.Deserialize<CandlesApiResponse>(responseStr,
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



        let getActiveContracts(): Async<Contract array> = 
            async{
                use flurlClient = FlurlHelper.getFlurlClient Log.Logger true

                let request = flurlClient.Request()
                                .AppendPathSegments("api", "v1", "contracts", "active")

                let signedRequest = FlurlHelper.signRequest
                                        request
                                        (Credentials.readCredentials())
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