namespace Analytics.Indicators

module Tilson = 

    let ema (prices: decimal[]) (period: int) (index: int) = 
        if index - period + 1 < 0 then failwith "Not enough data to calculate EMA"

        let multiplier = 2.0m / decimal (period + 1)
        let mutable ema = prices.[index]

        for i in index - 1 .. index - period + 1 .. -1 do
            ema <- (prices.[i] * multiplier) + ema * (1.0m - multiplier)
        ema


    let calculateT3 (prices: decimal[]) (period: int) (volumeFactor: decimal) =
        let length = Array.length prices
        let e1 = Array.zeroCreate<decimal> length
        let e2 = Array.zeroCreate<decimal> length
        let e3 = Array.zeroCreate<decimal> length
        let e4 = Array.zeroCreate<decimal> length
        let e5 = Array.zeroCreate<decimal> length
        let e6 = Array.zeroCreate<decimal> length
        let t3 = Array.zeroCreate<decimal> length

        //fill the elements before period with original values
        for i in 0 .. period - 1 do
            e1.[i] <- prices.[i]
            e2.[i] <- prices.[i]
            e3.[i] <- prices.[i]
            e4.[i] <- prices.[i]
            e5.[i] <- prices.[i]
            e6.[i] <- prices.[i]
            t3.[i] <- prices.[i]
        
        let fillOriginalUntilPeriod arr = 
            Array.blit prices 0 arr 0 period
        
        [|e1; e2; e3; e4; e5; e6|] |> Array.iter fillOriginalUntilPeriod


        for i in period - 1 .. length - 1 do
            e1.[i] <- ema prices period i
            e2.[i] <- ema e1 period i
            e3.[i] <- ema e2 period i
            e4.[i] <- ema e3 period i
            e5.[i] <- ema e4 period i
            e6.[i] <- ema e5 period i

            let c1 = -volumeFactor * volumeFactor * volumeFactor
            let c2 = 3.0m * volumeFactor * volumeFactor + 3.0m * volumeFactor * volumeFactor * volumeFactor
            let c3 = -6.0m * volumeFactor * volumeFactor - 3.0m * volumeFactor - 3.0m * volumeFactor * volumeFactor * volumeFactor
            let c4 = 1.0m + 3.0m * volumeFactor + volumeFactor * volumeFactor * volumeFactor + 3.0m * volumeFactor * volumeFactor

            t3.[i] <- c1 * e6.[i] + c2 * e5.[i] + c3 * e4.[i] + c4 * e3.[i]

        t3


    // // prices, 5, 0.5
    // public static decimal[] CalculateT3_Version2(decimal[] prices, int length, decimal volumeFactor)
    // {
    //     int emaLength = length;

    //     decimal[] e1 = new decimal[prices.Length];
    //     decimal[] e2 = new decimal[prices.Length];
    //     decimal[] e3 = new decimal[prices.Length];
    //     decimal[] e4 = new decimal[prices.Length];
    //     decimal[] e5 = new decimal[prices.Length];
    //     decimal[] e6 = new decimal[prices.Length];
    //     decimal[] t3 = new decimal[prices.Length];

    //     for (int i = 0; i < prices.Length; i++)
    //     {
    //         if (i == 0)
    //         {
    //             // For the first element, EMA is same as the price
    //             e1[i] = prices[i];
    //             e2[i] = prices[i];
    //             e3[i] = prices[i];
    //             e4[i] = prices[i];
    //             e5[i] = prices[i];
    //             e6[i] = prices[i];
    //             t3[i] = prices[i];
    //         }
    //         else
    //         {
    //             // Calculate EMA at each step
    //             decimal c1 = 2.0m / (emaLength + 1);
    //             decimal c2 = 1 - c1;

    //             e1[i] = c1 * prices[i] + c2 * e1[i - 1];
    //             e2[i] = c1 * e1[i] + c2 * e2[i - 1];
    //             e3[i] = c1 * e2[i] + c2 * e3[i - 1];
    //             e4[i] = c1 * e3[i] + c2 * e4[i - 1];
    //             e5[i] = c1 * e4[i] + c2 * e5[i - 1];
    //             e6[i] = c1 * e5[i] + c2 * e6[i - 1];

    //             // Calculate T3
    //             decimal v = volumeFactor * volumeFactor;
    //             t3[i] = (1 - v) * e6[i] + v * t3[i - 1];
    //         }
    //     }
    //     return t3;
    // }