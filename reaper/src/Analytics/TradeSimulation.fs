namespace Analytics

module TradeSimulation = 
    open Analytics.Types.TradeNs
    open Analytics.Types.MarketNs

    let amountWithProfit (tradeState: TradeState) (currentPrice: decimal) = 
        if tradeState.Side = SignalType.Buy then
            tradeState.TradeCapital + tradeState.Assets * (currentPrice - tradeState.EntryPrice)
        else
            tradeState.TradeCapital +  tradeState.Assets * (tradeState.EntryPrice - currentPrice)


    let tryBuy (actionToTake: SignalType) (currentPrice: decimal) (tradeState: TradeState) =
        if tradeState.Side <> SignalType.Buy && actionToTake = Buy then

            let assets = tradeState.TradeCapital / currentPrice

            {
                Side = SignalType.Buy
                Assets = assets
                TradeCapital = amountWithProfit tradeState currentPrice
                EntryPrice = currentPrice
            } : TradeState
        else
            tradeState



    let trySell (actionToTake: SignalType) (currentPrice: decimal) (tradeState: TradeState) =
        if tradeState.Side <> SignalType.Sell && actionToTake = Sell then
            let assets = tradeState.TradeCapital / currentPrice

            {
                Side = SignalType.Sell
                Assets = assets
                TradeCapital = amountWithProfit tradeState currentPrice
                EntryPrice = currentPrice
            } : TradeState
        else
            tradeState


    let trade (tradeState: TradeState) (actionToTake: SignalType) (currentPrice: decimal) =
        match actionToTake with
        | Buy -> tryBuy actionToTake currentPrice tradeState
        | Sell -> trySell actionToTake currentPrice tradeState
        | SignalUndefined -> tradeState


    let exit_trade (tradeState: TradeState) (lastCandle: Candle) = 
        let profit = 0m
        if tradeState.Side = SignalType.Buy then
            { tradeState with  
                    TradeCapital = 
                        tradeState.TradeCapital + tradeState.Assets * (lastCandle.Close - tradeState.EntryPrice) }
        else
            { tradeState with 
                    TradeCapital = 
                        tradeState.TradeCapital +  tradeState.Assets * (tradeState.EntryPrice - lastCandle.Close) }