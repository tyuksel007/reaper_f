#r "../../bin/Debug/net8.0/Analytics.dll"
#r "../../../Exchanges/Kucoin/bin/Debug/net8.0/Kucoin.dll"

#load "../../../../.paket/load/net8.0/Npgsql.fsx"
#load "../../../../.paket/load/net8.0/Dapper.fsx"

#load "../../../../.paket/load/net8.0/Serilog.fsx"
#load "../../../../.paket/load/net8.0/Serilog.Sinks.Console.fsx"
#load "../../../../.paket/load/net8.0/Serilog.Sinks.File.fsx"
#load "../../../../.paket/load/net8.0/Flurl.Http.fsx"
#load "../../../../.paket/load/net8.0/MathNet.Numerics.fsx"
#load "../../../../.paket/load/net8.0/FSharp.Control.AsyncSeq.fsx"

open System
open Analytics.Types.MarketNs
open Analytics.Types.AnalysisNs
open FSharp.Control



let contract: Contract = {Symbol = "XMRUSDTM"; MarkPrice = -1.m}
let intervalInMins = 5

let get_candles () = 
    async{
        let start = DateTime.Now.AddMonths(-5).ToString(Analytics.TimeUtils.timeFormat)
        let end_ = Some (DateTime.Now.ToString(Analytics.TimeUtils.timeFormat))
        let candles = Kucoin.Services.MarketDataService.get_candles contract.Symbol start end_ intervalInMins
        return candles
    }


let save_candles (candles_seq: AsyncSeq<Kucoin.Types.MarketDataApiNs.Candle array>) = 
    async {
        do! candles_seq
            |> AsyncSeq.iterAsync(fun candles -> 
                async {
                    let analytics_candles = 
                        candles |> Array.map(fun (entity : Kucoin.Types.MarketDataApiNs.Candle)->
                                {
                                    Time = entity.Time 
                                    Close = entity.Close 
                                    Open = entity.Open 
                                    High = entity.High
                                    Low = entity.Low 
                                    Volume = entity.Volume
                                } : Analytics.Types.MarketNs.Candle)
                        
                    Analytics.Database.CandleOps.save_candles contract.Symbol intervalInMins analytics_candles
                    printfn "Candles saved successfully"
                }
            )
    }

//save_candles (get_candles())
let print_klines () = async {

    let! klines = get_candles()

    let str = System.Text.Json.JsonSerializer.Serialize(klines)
    printfn $"Klines-length: {str} \n"

    let mutable count = 0
    do! klines
        |> AsyncSeq.iter(fun candles -> 
            for candle in candles do
                count <- count + 1
        ) 
    printfn $"Total candles: {count}"
}

async { 
    do! print_klines() 
    printfn "Done"
} |> Async.RunSynchronously


let get_orders () =
    Analytics.Patterns.Broadening_Bottoms.run_analysis contract 120 


let print_pattern_orders (orders: AsyncSeq<PatternOrder array>) = 
    printfn "Pattern Orders: \n"
    orders 
        |> AsyncSeq.iter(fun orders -> 
            for order in orders do
                let json_order = System.Text.Json.JsonSerializer.Serialize(order)
                printfn "Order: %s" json_order
        )
        |> Async.RunSynchronously


print_pattern_orders (get_orders())
        