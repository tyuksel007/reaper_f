namespace Analytics.Types

module TradeNs = 

    [<Struct>]
    type SignalType = 
        | Buy
        | Sell
        | SignalUndefined



    [<Struct>]
    type TradeState ={
        Side: SignalType
        Assets: decimal
        TradeCapital: decimal
        EntryPrice: decimal
    }
