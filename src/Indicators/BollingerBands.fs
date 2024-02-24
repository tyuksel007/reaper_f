namespace Indicators

module BollingerBands = 

    let sma (prices: decimal[]) (period: int) (index: int) = 
        if index - period + 1 < 0 then failwith "Not enough data to calculate SMA"
        let sum = 
            prices
            |> Array.take period
            |> Array.sum
        sum / decimal period

    let standardDeviation (prices: decimal[]) (period: int) (index: int) = 
        let mean = sma prices period index

        let sumOfSquares = 
            prices
            |> Array.take period
            |> Array.map (fun p -> p - mean)
            |> Array.sumBy (fun d -> d * d)

        decimal (sqrt (float (sumOfSquares / decimal period)))



    let calculateBollingerBands (prices: decimal[]) (period: int) (standardDeviationMultiplier: decimal) = 
        if Array.length prices < period then failwith "Not enough data to calculate Bollinger Bands"

        let upperBand = Array.zeroCreate<decimal> (Array.length prices)
        let middleBand = Array.zeroCreate<decimal> (Array.length prices)
        let lowerBand = Array.zeroCreate<decimal> (Array.length prices)

        for i in period - 1 .. Array.length prices - 1 do
            let sma = sma prices period i
            let stdDev = standardDeviation prices period i

            upperBand.[i] <- sma + stdDev * standardDeviationMultiplier
            middleBand.[i] <- sma
            lowerBand.[i] <- sma - stdDev * standardDeviationMultiplier

        (upperBand, middleBand, lowerBand)