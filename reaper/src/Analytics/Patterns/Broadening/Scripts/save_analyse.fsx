#r "../../../bin/Debug/net8.0/Analytics.dll"
#r "../../../../Exchanges/Binance/bin/Debug/net8.0/Binance.dll"
#r "../../../../Exchanges/Kucoin/bin/Debug/net8.0/Kucoin.dll"


#load "../../../../../.paket/load/net8.0/Npgsql.fsx"
#load "../../../../../.paket/load/net8.0/Dapper.fsx"

#load "../../../../../.paket/load/net8.0/Serilog.fsx"
#load "../../../../../.paket/load/net8.0/Serilog.Sinks.Console.fsx"
#load "../../../../../.paket/load/net8.0/Serilog.Sinks.File.fsx"
#load "../../../../../.paket/load/net8.0/Flurl.Http.fsx"
#load "../../../../../.paket/load/net8.0/MathNet.Numerics.fsx"
#load "../../../../../.paket/load/net8.0/FSharp.Control.AsyncSeq.fsx"


open System
open Analytics.Types.MarketNs
open Analytics.Types.AnalysisNs
open FSharp.Control



let contract: Contract = {Symbol = "LUNAUSDT"; MarkPrice = -1.m}
let intervalInMins = 1440

let get_kucoin_candles () = 
    async{
        let start = DateTime.Now.AddMonths(-5).ToString(Analytics.TimeUtils.timeFormat)
        let end_ = Some (DateTime.Now.ToString(Analytics.TimeUtils.timeFormat))

        return Kucoin.Services.MarketDataService.get_candles contract.Symbol start end_ intervalInMins
            |> AsyncSeq.map(fun (candles : Kucoin.Types.MarketDataApiNs.Candle array) -> 
                candles 
                    |> Array.map(fun (entity : Kucoin.Types.MarketDataApiNs.Candle)->
                    {
                        Time = entity.Time 
                        Close = entity.Close 
                        Open = entity.Open 
                        High = entity.High
                        Low = entity.Low 
                        Volume = entity.Volume
                    } : Analytics.Types.MarketNs.Candle)
            )
    }

let get_binance_candles () = 
    async{
        let start = DateTime.Now.AddMonths(-5).ToString(Analytics.TimeUtils.timeFormat)
        let end_ = Some (DateTime.Now.ToString(Analytics.TimeUtils.timeFormat))

        return Binance.Services.MarketDataService.get_candles contract.Symbol start end_ intervalInMins
            |> AsyncSeq.map(fun (candles : Binance.Types.MarketDataApiNs.BinanceCandle array) -> 
                candles 
                    |> Array.map(fun (entity : Binance.Types.MarketDataApiNs.BinanceCandle)->
                    {
                        Time = entity.OpenTime 
                        Close = entity.ClosePrice 
                        Open = entity.OpenPrice 
                        High = entity.HighPrice
                        Low = entity.LowPrice 
                        Volume = entity.Volume
                    } : Analytics.Types.MarketNs.Candle)
            )
    }

let save_candles (candles_seq: AsyncSeq<Candle array>) = 
    async {
        do! candles_seq
            |> AsyncSeq.iterAsync(fun candles -> 
                async {
                    do! Analytics.Database.CandleOps.save_candles contract.Symbol intervalInMins candles
                    printfn "Candles saved successfully"
                }
            )
    }

// async { 
//     let candles_seq = get_binance_candles() |> Async.RunSynchronously
//     do! save_candles candles_seq 
//     printfn "Done"
// } |> Async.RunSynchronously


let print_klines () = async {

    printfn "Getting klines from Kucoin!"
    printfn "starting timer"
    let stop_watch = System.Diagnostics.Stopwatch.StartNew()

    let! klines = get_binance_candles()


    let str = System.Text.Json.JsonSerializer.Serialize(klines)
    printfn $"Klines-length: {str} \n"

    let mutable count = 0
    do! klines
        |> AsyncSeq.iter(fun candles -> 
            for candle in candles do
                count <- count + 1
        ) 
    printfn $"Total candles: {count}"
    printfn $"Time taken: {stop_watch.ElapsedMilliseconds} ms"
}

// async { 
//     do! print_klines() 
//     printfn "Done"
// } |> Async.RunSynchronously


        