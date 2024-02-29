namespace Analytics.Indicators

module MovingAvg =
    open System
    open Analytics.Types

    let sma (data: decimal array) (period: int) =
        data
        |> Array.mapi (fun i _  -> 
            if i >= period then
                let slice = data[(i - period) .. period]
                slice |> Array.average
            else
                0M
        )


    //exponential moving average
    let ema (prices: decimal[]) (period: int) =
        if period > prices.Length then failwith "Not enough data to calculate EMA"

        let smoothing = 2.0m / decimal (period + 1)

        let emaArr = Array.zeroCreate<decimal> prices.Length

        emaArr
            |> Array.iteri (fun i _ -> 
                if i >= period then
                    emaArr.[i] <- prices.[i] * smoothing + emaArr.[i-1] * (1.0m - smoothing)
            )

        emaArr

        
    
    // double exponential moving average
    // formula => DEMA = (2 * EMA(n)) - EMA(EMA(n))
    let dema (prices: decimal[]) (period: int) =

        if period > prices.Length then failwith "Not enough data to calculate DEMA"

        let emaOrigin = ema prices period
        let emaOfEma = ema emaOrigin period

        Array.mapi (fun i _ -> 2.0m * emaOrigin.[i] - emaOfEma.[i]) prices


    // triple exponential moving average
    // formula => TEMA = (3 * EMA(n)) - (3 * EMA(EMA(n))) + EMA(EMA(EMA(n)))
    let tema (prices: decimal[]) (period: int) =

        if period > prices.Length then failwith "Not enough data to calculate TEMA"

        let emaOrigin = ema prices period
        let emaOfEma = ema emaOrigin period
        let emaOfEmaOfEma = ema emaOfEma period

        Array.mapi (fun i _ -> 3.0m * emaOrigin.[i] - 3.0m * emaOfEma.[i] + emaOfEmaOfEma.[i]) prices

    // linear weighted moving average
    // recent data given more weight
    // old data given less weight
    let lwma (prices: decimal[]) (period: int) =

        if period > prices.Length then failwith "Not enough data to calculate LWMA"

        let periodSum = period * (period + 1) / 2

        let lwmaArr = Array.zeroCreate<decimal> prices.Length

        lwmaArr
            |> Array.iteri (fun i _ -> 
                if i >= period then
                    let sum = 
                        prices.[i-period..i]
                            |> Array.mapi (fun j p -> p * (decimal (j+1)))// + 1 for zero-based index
                            |> Array.sum
                    lwmaArr.[i] <- sum / decimal periodSum
                )   

        lwmaArr

    //  volume-weighted moving average price
    //  volume affects the average price calculation
    //  VWAP = Cumulative Typical Price x Volume/Cumulative Volume
    //  Where Typical Price = High price + Low price + Closing Price/3
    //  Cumulative = total since the trading session opened.
    let vwmap (candles: Candle[]) (period: int): decimal array =

        let typicalPrice candle = 
            (candle.High + candle.Low + candle.Close) / 3.0m

        let resultArr  =
            candles
                |> Array.mapi (fun i c ->
                    if i >= period then

                        let slice = candles.[i - period .. i]

                        let sumTypicalPriceVolume = 
                            slice
                            |> Array.sumBy (fun c -> typicalPrice c * c.Volume)

                        let sumVolume = 
                            slice
                            |> Array.sumBy (fun c -> c.Volume)

                        sumTypicalPriceVolume / decimal sumVolume
                    else
                        0M 
                )
        resultArr


    let standardDeviation (arr: decimal array) (period: int) = 
        if period > arr.Length then failwith "Not enough data to calculate standard deviation"

        let smas = sma arr period

        let deviations = 
            arr
            |> Array.mapi (fun i _ -> 
                if i >= period - 1 then
                    let slice = arr.[i - period + 1 .. i]
                    let mean = smas.[i]
                    let variance = 
                        slice 
                        |> Array.map (fun p -> (p - mean) * (p - mean)) 
                        |> Array.average

                    decimal (Math.Sqrt (double variance))
                else
                    0M
            )
        
        deviations

