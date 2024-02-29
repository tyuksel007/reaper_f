namespace Analytics.Indicators

module BollingerBands = 

    let calculateBollingerBands (prices: decimal[]) (period: int) (standardDeviationMultiplier: decimal) = 
    
        if Array.length prices < period then failwith "Not enough data to calculate Bollinger Bands"

        let upperBand = Array.zeroCreate<decimal> (Array.length prices)
        let middleBand = Array.zeroCreate<decimal> (Array.length prices)
        let lowerBand = Array.zeroCreate<decimal> (Array.length prices)
        

        let smas = MovingAvg.sma prices period
        let standardDeviations = MovingAvg.standardDeviation prices period
        prices
            |> Array.iteri (fun i p -> 
                upperBand.[i] <- smas.[i] + (standardDeviationMultiplier * standardDeviations.[i])
                middleBand.[i] <- smas.[i]
                lowerBand.[i] <- smas.[i] - (standardDeviationMultiplier * standardDeviations.[i])
            )

        (upperBand, middleBand, lowerBand)
