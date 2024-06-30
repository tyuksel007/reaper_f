#r "../../../bin/Debug/net8.0/Analytics.dll"
#r "../../../../Exchanges/Binance/bin/Debug/net8.0/Binance.dll"

#load "../../../../../.paket/load/net8.0/Plotly.NET.fsx"

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
open FSharp.Control
open Plotly.NET
open Analytics.Types.AnalysisNs

let contract: Contract = {Symbol = "LUNAUSDT"; MarkPrice = -1.m}
let interval_in_mins = 1440


let read_candles () = async {
    let candles = 
        Analytics.Database.CandleOps.read_candles 
            contract.Symbol 
            interval_in_mins 
            None 
            None

    return candles
}

let read_analysis () = async {
    let analysis = 
        Analytics.Database.AnalysisOps.read_analysis 
            contract.Symbol 
            interval_in_mins 
            None
            None

    return analysis
}


let candle_chart (candles: Candle array) = async {
    let prepare_for_chart (candles_: Candle array) = 
        candles_
        |> Array.map (fun c -> 
            let time = DateTimeOffset.FromUnixTimeMilliseconds(c.Time).DateTime
            let stockData: TraceObjects.StockData = {
                Open = float c.Open
                High = float c.High 
                Low = float c.Low
                Close = float c.Close
                } 
            // (time, c.Open, c.High, c.Low, c.Close))
            (time, stockData))


    // let! candle_seq = read_candles() 
    // let! candles_arr = 
    //     candle_seq
    //         |> AsyncSeq.fold (fun acc candles_->
    //             Array.append acc (candles_ |> Array.ofSeq)
    //         ) [||]

    return candles |> prepare_for_chart |> Chart.Candlestick
}


let calculate_line_points (slope: float) (intercept: float) (x_values: float list) = 
    x_values |> List.map (fun x -> intercept + slope * x)

let add_lines_to_chart (chart: GenericChart.GenericChart) (x_values: float seq) (line_points: float seq) (name: string) (color: string) =
    Chart.Line(x_values, line_points, Name = name, ShowLegend = true)
    |> Chart.withLineStyle(Color = Color.fromString(color), Width = 2.0)
    |> fun line_trace -> Chart.combine [chart; line_trace]

let add_pivots_to_chart (chart: GenericChart.GenericChart) 
        (name: string) (color: string) (low_points: float seq) (high_points: float seq) =

    Chart.Scatter(low_points, high_points, mode = StyleParam.Mode.Markers, Name = name)
    |> Chart.withMarkerStyle(Size = 10, Color = Color.fromString(color))
    |> fun points_trace -> Chart.combine [chart; points_trace]



let print_pattern_analysis (analysis_arr: AnalysisResult array) = 
    printfn "analysis result: \n"
    for analysis in analysis_arr do
        let json_order = System.Text.Json.JsonSerializer.Serialize(analysis)
        printfn "analysis: %s" json_order

let show () = 

    let candle_seq = 
        read_candles() 
        |> Async.RunSynchronously
    
    let analyse (candles: Candle array) = 
        read_analysis(candles) 
        |> Async.RunSynchronously

    candle_seq
    |> AsyncSeq.iter(fun candles ->
        candle_chart candles 
        |> Async.RunSynchronously 
        // |> fun chart -> add_pivots_to_chart chart "Pivots" "red" low_points high_points
        // |> fun chart -> add_lines_to_chart chart [1.0; 2.0; 3.0] [1.0; 2.0; 3.0] "Line" "blue"
        |> Chart.show
    )





    // 1. Read candles from the database by chunk of 200
    // 2. read analysis from the database by chunk of 200
    // 3. prepare candlestick chart
    // 4. prepare analysis chart => lines, pivots, breakout points
    // 5. combine charts
    // 6. save each chart of chunk of 200 candles and analysis to a file with naming template
    // naming template: {symbol}_{interval}_{chunk_number}_{from_as_datetime}_{to_as_datetime}.html
