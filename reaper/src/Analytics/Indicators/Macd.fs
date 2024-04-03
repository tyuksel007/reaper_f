namespace Analytics.Indicators

module Macd = 

    let ema (prices: decimal[]) (period: int) (index: int) = 
        if index - period + 1 < 0 then failwith "Not enough data to calculate EMA"

        let multiplier = 2.0m / decimal (period + 1)
        let mutable ema = prices[index]

        for i in index - 1 .. index - period + 1 .. -1 do
            ema <- (prices.[i] - ema) * multiplier + ema
        ema

    let calculateMacd (prices: decimal[]) (fastLength: int) (slowLength: int) (signalSmoothing: int) = 
        let macdLine = Array.zeroCreate<decimal> (Array.length prices)
        let signalLine = Array.zeroCreate<decimal> (Array.length prices)

        for i in slowLength - 1 .. Array.length prices - 1 do
            let fastEma = ema prices fastLength i
            let slowEma = ema prices slowLength i

            macdLine.[i] <- fastEma - slowEma
            signalLine.[i] <- ema macdLine signalSmoothing i

        (macdLine, signalLine)