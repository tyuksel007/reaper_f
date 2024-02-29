namespace Analytics

module TradeSimulation = 
    open Types

    let withProfitAmount (tradeState: TradeState) (currentPrice: decimal) = 
        if tradeState.CurrentPosition = SignalType.Buy then
            tradeState.TradeCapital + tradeState.Assets * (currentPrice - tradeState.EntryPrice)
        else
            tradeState.TradeCapital +  tradeState.Assets * (tradeState.EntryPrice - currentPrice)


    let tryBuy (actionToTake: SignalType) (currentPrice: decimal) (tradeState: TradeState) =
        if tradeState.CurrentPosition <> SignalType.Buy && actionToTake = Buy then

            let assets = tradeState.TradeCapital / currentPrice

            {
                CurrentPosition = SignalType.Buy
                Assets = assets
                TradeCapital = withProfitAmount tradeState currentPrice
                EntryPrice = currentPrice
            } : TradeState
        else
            tradeState



    let trySell (actionToTake: SignalType) (currentPrice: decimal) (tradeState: TradeState) =
        if tradeState.CurrentPosition <> SignalType.Sell && actionToTake = Sell then
            let assets = tradeState.TradeCapital / currentPrice

            {
                CurrentPosition = SignalType.Sell
                Assets = assets
                TradeCapital = withProfitAmount tradeState currentPrice
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
        if tradeState.CurrentPosition = SignalType.Buy then
            { tradeState with  
                    TradeCapital = 
                        tradeState.TradeCapital + tradeState.Assets * (lastCandle.Close - tradeState.EntryPrice) }
        else
            { tradeState with 
                    TradeCapital = 
                        tradeState.TradeCapital +  tradeState.Assets * (tradeState.EntryPrice - lastCandle.Close) }