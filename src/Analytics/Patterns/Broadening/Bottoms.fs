namespace Analytics.Patterns

module Broadening_Bottoms = 
    open MathNet.Numerics
    open Analytics.Types
    open System.Data.SQLite
    open System
    open Analytics
    open Analytics.Patterns.Broadening.DbOps
    open Analytics.Patterns.Broadening

    // calculate regression line
    // calculate pivot points
    // collect channels
    // calculate the width of the channel
    // calculate the slope of the channel
    // detect breakout


    [<Struct>]
    type RegressionValues = {
        SlopeHigh: float
        InterceptHigh: float
        RSquaredHigh: float
        SlopwLow: float
        InterceptLow: float
        RSquaredLow: float
    }


    [<Struct>]
    type BreakOutData = {
        Index: int
        Price: decimal
        Signal: SignalType
        Time: int64
    }





    let regression (xValues: float[]) (yValues: float[]) =
        let struct (intercept, slope)  = Fit.Line(xValues, yValues)
        let rSquared = GoodnessOfFit.RSquared(
            xValues |> Array.map (fun x -> intercept + slope * x),
            yValues
        )
        (slope, intercept, rSquared)




    let trade_entry_point (low: Pivot option) (high: Pivot option) : decimal = 
        match low, high with
        | Some low, Some high -> -1m
        | Some low, None -> low.Price 
        | None, Some high -> high.Price 
        | None, None -> -1m



    let get_pivot (candles: Candle[]) (window: int) (curentIdx: int) = 
        if curentIdx - window < 0 && curentIdx + window >= candles.Length then
            None
        else
            let subCandles = candles.[curentIdx - window..curentIdx + window]

            let pivotHigh = 
                subCandles
                |> Array.maxBy (fun x -> x.High)

            let pivotLow = 
                subCandles
                |> Array.minBy (fun x -> x.Low)

            let pivot =
                if candles.[curentIdx].High = pivotHigh.High then
                    Some {Price = pivotHigh.High; PivotType = High; Index = curentIdx; Time = pivotHigh.Time}
                elif candles.[curentIdx].Low = pivotLow.Low then
                    Some {Price = pivotLow.Low; PivotType = Low; Index = curentIdx; Time = pivotLow.Time}
                else
                    None 
            pivot
    


    let collect_channel (pivots: Pivot[]) =
        let lows = 
            pivots
            |> Array.filter (fun x -> x.PivotType = Low)

        let highs = 
            pivots
            |> Array.filter (fun x -> x.PivotType = High)
            
        if lows.Length >=2 && highs.Length >= 2 then
            let (lslope, lintercept, lrsquared) = 
                regression 
                    (lows |> Array.map (fun x -> x.Price |> float))
                    (lows |> Array.map (fun x -> x.Index |> float))
            let (hslope, hintercept, hrsquared) = 
                regression 
                    (highs |> Array.map (fun x -> x.Price |> float))
                    (highs |> Array.map (fun x -> x.Index |> float))

            Some 
                {
                    SlopeHigh = hslope
                    InterceptHigh = hintercept
                    RSquaredHigh = hrsquared; 
                    SlopwLow = lslope; 
                    InterceptLow = lintercept; 
                    RSquaredLow = lrsquared
                }
        else
            None


    let detect_breakout (channel: RegressionValues )
        (candles: Candle[])  
        (current: int) 
        (backCandles: int) 
        (window: int)  =

        let breakout = {Index = current; Price = candles.[current].Close; Signal = SignalUndefined; Time = 0L}

        if current - backCandles - window < 0 || current + 20 >= Array.length candles then
            breakout
        else
            let prevCandle = candles.[current - 1]
            let currentCandle = candles.[current]

            let channelLow (i: int) = channel.SlopwLow * (float i) + channel.InterceptLow
            let channelHigh (i: int) = channel.SlopeHigh * (float i) + channel.InterceptHigh

            let highBreakout = float prevCandle.High > channelLow (current - 1)
                                    && float prevCandle.Close < channelLow (current - 1)
                                    && float currentCandle.Open < channelLow (current)
                                    && float currentCandle.Close < channelLow (current)

            let lowBreakout = float prevCandle.Low < channelHigh (current - 1)
                                    && float currentCandle.Close > channelHigh (current - 1)
                                    && float currentCandle.Open > channelHigh (current)
                                    && float currentCandle.Close > channelHigh (current)

            if highBreakout then
                {breakout with Signal = Buy; Price = currentCandle.Close; Index = current; Time = currentCandle.Time}
            elif lowBreakout then
                {breakout with Signal = Sell; Price = currentCandle.Close; Index = current; Time = currentCandle.Time}
            else
                breakout


    let get_pivots (candles: Candle array) (window: int) = 
        candles
        |> Array.mapi (fun i x -> get_pivot candles window i)
        |> Array.choose id


    let get_channels (pivots: Pivot array) = 

        let channelSize = 3

        let pivotsChunks = 
            pivots
            |> Array.splitInto channelSize

        let someChannels = 
            pivotsChunks
                |> Array.map(fun x -> 
                    collect_channel x
                ) 
        someChannels



    let with_channel (channels: RegressionValues array) (currentIdx: int) (candlesLength: int) = 
        let channelRange = candlesLength / channels.Length
        let channelIndex = currentIdx / channelRange

        if channelIndex >= channels.Length then
            channels.[channels.Length - 1]
        else
            channels.[channelIndex]

        

    let save_stats (connection: SQLiteConnection) 
        (symbol: string)
        (candles: Candle array) 
        (channels: RegressionValues array) 
        (pivots: Pivot array) 
        (breakouts: BreakOutData array) =

        try_create_table connection

        let low_regression (i: int) =  
            let ch = with_channel channels i candles.Length
            decimal (ch.SlopwLow * (float i) + ch.InterceptLow)

        let high_regression (i: int) =
            let ch = with_channel channels i candles.Length
            decimal (ch.SlopeHigh * (float i) + ch.InterceptHigh)



        let mutable tradeState =
            {
                CurrentPosition = SignalUndefined
                Assets = 0m
                TradeCapital = 100m//testing..........
                EntryPrice = 0m
            }

        candles
            |> Array.iteri(fun i  candle -> 

                    let breakOutAction =  breakouts.[i].Signal
                    tradeState <- TradeSimulation.trade tradeState breakOutAction candle.Close

                    let pivotType = 
                        match pivots 
                            |> Array.tryFind (fun x -> x.Index = i) with
                            | Some pivot -> pivot.PivotType
                            | None -> PivotType.PivotUndefined

                    let pivotPrice = 
                        match pivots |> Array.tryFind (fun x -> x.Index = i) with
                        | Some pivot -> pivot.Price
                        | None -> -1m


                    insert_broadeningDto connection  
                        ({
                            Symbol = symbol
                            Time = TimeUtils.utcToLocalTime candles.[i].Time
                            PivotType = pivotType.ToString()
                            PivotPrice = pivotPrice
                            ChannelLow = low_regression i
                            ChannelHigh = high_regression i
                            BreakoutSignal = breakOutAction.ToString()
                            TradeCapital = tradeState.TradeCapital
                        } : Types.BroadeningBottomDto)
                )

        tradeState <- TradeSimulation.exit_trade tradeState candles.[candles.Length - 1]
        _logger.writeMessageToFile "broadening_bottoms" $"Final Trade Capital: {tradeState.TradeCapital}"
        ()




    let save (symbol: string) (candles: Candle array) =

        use connection = new SQLiteConnection(Analytics.Database.Connection.connectionStringFile)
        connection.Open()

        Database.CandleOps.save_candles connection symbol candles


        let pivots = get_pivots candles 5

        let someChannels = get_channels pivots

        let channelsExists = 
            someChannels
            |> Array.exists (fun x -> x.IsSome)
        
        if not channelsExists then
            ()
        else

            let backCandles = 10
            let window = 3
            let channels = someChannels |> Array.choose id 

            let breakouts = 
                candles
                |> Array.mapi (fun i x -> 
                    let channel = with_channel channels i candles.Length
                    detect_breakout channel candles i backCandles window
                )

            save_stats connection symbol candles channels pivots breakouts

        read_broadeningDto connection