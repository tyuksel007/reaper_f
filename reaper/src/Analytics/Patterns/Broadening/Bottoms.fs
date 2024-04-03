namespace Analytics.Patterns

module Broadening_Bottoms = 
    open MathNet.Numerics
    open System
    open Analytics.Types.MarketNs
    open Analytics.Types.TradeNs
    open Analytics.Types.AnalysisNs
    open Analytics
    open FSharp.Control

    let private linear_regression (xValues: float[]) (yValues: float[]) : RegressionValues =
        let struct (intercept, slope)  = Fit.Line(xValues, yValues)
        let rSquared = GoodnessOfFit.RSquared(
            xValues |> Array.map (fun x -> intercept + slope * x),
            yValues
        )
        { Slope =  slope;  Intercept = intercept; RSquared =  rSquared }

    let private get_high_points (candles: Candle array) = 
        let period = 80
        let window = 8
        let mutable highPoints = Array.zeroCreate<Candle> period

        candles[candles.Length - period - 1..candles.Length - 1]
        |> Array.iteri (fun i candle -> 
            let maxCandle = candles.[i..i+window] |> Array.maxBy (fun x -> x.High)
            highPoints.[i] <- maxCandle
        )
        highPoints



    let private get_low_points (candles: Candle array) = 
        let period = 80
        let window = 8
        let mutable lowPoints = Array.zeroCreate<Candle> period

        candles[candles.Length - period - 1..candles.Length - 1]
        |> Array.iteri (fun i candle -> 
            let minCandle = candles.[i..i+window] |> Array.minBy (fun x -> x.Low)
            lowPoints.[i] <- minCandle
        )
        lowPoints


    let private upper_line (highPoints: Candle array) : RegressionValues = 
        let xValues = highPoints |> Array.map (fun x -> float x.Time)
        let yValues = highPoints |> Array.map (fun x -> float x.High)
        linear_regression xValues yValues

    let private lower_line (lowPoints: Candle array) : RegressionValues = 
        let xValues = lowPoints |> Array.map (fun x -> float x.Time)
        let yValues = lowPoints |> Array.map (fun x -> float x.Low)
        linear_regression xValues yValues

    let fit_into line value = line.Intercept + line.Slope * value


    let private is_delta_increasing (chunk: Candle array) (upper_line: RegressionValues) (lower_line: RegressionValues) = 

        let delta_arr: bool array = Array.zeroCreate<bool> chunk.Length

        for i = 1 to chunk.Length - 1 do
            let current = chunk.[i]
            let prev = chunk.[i-1]

            let current_upper = fit_into upper_line (float current.Time)
            let prev_upper = fit_into upper_line (float prev.Time)

            let current_lower = fit_into lower_line (float current.Time)
            let prev_lower = fit_into lower_line (float prev.Time)

            if current_upper > prev_upper && current_lower < prev_lower then
                delta_arr[i] <- true
            else
                delta_arr[i] <- false


        delta_arr |> Array.forall (fun x -> x )


    // iterate over last 10 items
    // calculate upper and lower line value for i and i-1
    // check if current value of upper is bigger than prev.upper 
    // and current.low < prev.low
    let private find_breakout (chunk: Candle array) (intervalInMins: int) (contract: Contract) (upper_line: RegressionValues) (lower_line: RegressionValues) = 

        let is_expanding = is_delta_increasing chunk upper_line lower_line

        let last = chunk.Length - 1
        let secon_from_last = chunk.Length - 2
        let buy_signal = float (chunk.[last].Close) > fit_into upper_line (float chunk.[last].Time)
                                && float (chunk.[secon_from_last].Close) > fit_into upper_line (float chunk.[secon_from_last].Time)

        let profit_delta = fit_into upper_line (float chunk.[last].Time) 
                                - fit_into lower_line (float chunk.[last].Time)

        if is_expanding && buy_signal then
            {
                Symbol = contract.Symbol
                Time = chunk.[last].Time
                IntervalInMins = intervalInMins
                Pattern = "Broadening Bottoms"
                Signal = nameof SignalType.Buy
                EnterPrice = chunk[last].Close + chunk[last].Close * 0.01m
                TakeProfit = chunk[last].Close + decimal profit_delta
                StopLoss = chunk[last].Close - decimal profit_delta
            }
        else
            {
                Symbol = contract.Symbol
                Time = -1
                IntervalInMins = intervalInMins
                Pattern = "Broadening Bottoms"
                Signal = nameof SignalType.SignalUndefined
                EnterPrice = 0m
                TakeProfit = 0m
                StopLoss = 0m
            }


    let read_by_chunk (contract: Contract) (intervalInMins: int) =
        asyncSeq{
            let count_of_records = 1000// todo: get from db
            let chunkSize = 200
            let jump = count_of_records / chunkSize

            let mutable start = 0L//todod: first record.Time from db
            for i = 0 to jump do
                let end_ = start + int64((i+1) * chunkSize)
                let candles = Database.CandleOps.read_candles contract.Symbol intervalInMins start end_
                start <- end_

                yield candles
        }
    
    let run_analysis (contract: Contract) (interval: int) = 
        let analyse_fn (candles: Candle array) = 
            let chunkSize = 10
            let start = 10
            candles.[start..candles.Length-1]
                |> Array.mapi (fun i  _ -> 

                    let chunk = candles.[i-chunkSize..i]
                    let upper_line = get_high_points chunk |> upper_line
                    let lower_line = get_low_points chunk |> lower_line

                    let result = find_breakout chunk interval contract upper_line lower_line
                    result
                ) 
                |> Array.filter (fun x -> x.Signal <> nameof SignalType.SignalUndefined)

        let candles_seq = read_by_chunk contract interval 

        candles_seq 
            |> AsyncSeq.map(fun candles -> 
                let orders = 
                    candles
                        |> Array.ofSeq
                        |> Array.map(fun (entity : Database.CandleOps.CandleEntity)->
                        {
                            Time = entity.Time
                            Close = entity.Close
                            Open = entity.Open
                            High = entity.High
                            Low = entity.Low
                            Volume = entity.Volume
                        }) 
                        |> analyse_fn
                orders
            )
