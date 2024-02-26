namespace Indicators

module movingAvg =

    //  when shortSma > longSma, buy
    //  when shortSma < longSma, sell

    // when sma > price, buy
    // when sma < price, sell
    let sma (data: decimal[], period: int) = 

        if data.Length < period then failwith "Not enough data to calculate SMA"

        let smaArr = Array.zeroCreate<decimal> data.Length

        smaArr
            |> Array.iteri (fun i _ -> 
                if i >= period then
                    smaArr.[i] <- data.[i-period..i] |> Array.average
            )

        smaArr