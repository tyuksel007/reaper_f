namespace Binance.Services

module MarketDataService = 
    open FSharp.Control
    open System
    open Binance.Utils
    open Flurl.Http
    open System.Text.Json
    open Binance.Types.MarketDataApiNs
    let parseBinanceCandle (jsonElement: JsonElement): BinanceCandle =
        let array = jsonElement.EnumerateArray() |> Seq.toArray
        {
            OpenTime = array.[0].GetInt64()
            OpenPrice = decimal (array.[1].GetString())
            HighPrice = decimal (array.[2].GetString())
            LowPrice = decimal (array.[3].GetString())
            ClosePrice = decimal (array.[4].GetString())
            Volume = decimal (array.[5].GetString())
            CloseTime = array.[6].GetInt64()
            QuoteAssetVolume = decimal (array.[7].GetString())
            NumberOfTrades = array.[8].GetInt32()
            TakerBuyBaseAssetVolume = decimal (array.[9].GetString())
            TakerBuyQuoteAssetVolume = decimal (array.[10].GetString())
        }


    let private get_200_candles(symbol: string) (from: float) (to_: float) (interval: string) = 
        async{
            let queryParams = [
                "symbol", symbol
                "startTime", from.ToString()
                "endTime", to_.ToString()
                "limit", "200"
                "interval", interval
            ]

            use flurlClient = FlurlHelper.get_flurl_client (RLogger.get_default_rlogger())  true

            let request = flurlClient.Request()
                            .AppendPathSegments("api", "v3", "klines")
                            .SetQueryParams(queryParams)

            let updated_request = FlurlHelper.update 
                                    request 
                                    (Authorisation.read_credentials())
                                    "GET"
                                    (Some "")
                                    false
                                    
            try
                let! responseStr = updated_request.GetStringAsync() |> Async.AwaitTask

                return JsonDocument.Parse(responseStr).RootElement.EnumerateArray()
                    |> Seq.map parseBinanceCandle
                    |> Seq.toArray
            with
            | ex -> 
                printfn "Error: %s" ex.Message
                return [||]
        }


    let transform_interval (intervalInMins: int) =
        match intervalInMins with
        | 1 -> "1m"
        | 3 -> "3m"
        | 5 -> "5m"
        | 15 -> "15m"
        | 30 -> "30m"
        | 60 -> "1h"
        | 120 -> "2h"
        | 240 -> "4h"
        | 360 -> "6h"
        | 480 -> "8h"
        | 720 -> "12h"
        | 1440 -> "1d"
        | 4320 -> "3d"
        | 10080 -> "1w"
        | 43200 -> "1M"
        | _ -> 
            printfn "Invalid interval, using 1m"
            "1m"



    let get_candles (symbol: string) (startTime: string) (endTime: Option<string>) (interval_in_mins: int) = 
        asyncSeq{
            let endTimeValue = match endTime with
                                | Some et -> et
                                | None -> DateTime.Now.ToString(TimeHelper.timeFormat)

            let mutable from = startTime |> TimeHelper.toUtcEpoch
            let to_ = endTimeValue |> TimeHelper.toUtcEpoch

            let chunk_size = 200
            let delta_in_minutes = (to_ - from) / 1000.0 / 60.0
            let total_num_of_candles = delta_in_minutes / float interval_in_mins
            let count_of_iteration = int (ceil total_num_of_candles / float chunk_size)
            
            for i = 0 to count_of_iteration do
                let next_to = from + (float chunk_size * float interval_in_mins) * 1000.0 * 60.0
                let next= min next_to to_
                
                let! iter_candles = get_200_candles symbol from next (transform_interval interval_in_mins)
                from <- next_to
                yield iter_candles
        }

    let get_symbol_price () =
        ()

    let get_active_contracts () = 
        ()