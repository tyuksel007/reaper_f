namespace Analytics

module Types = 

    [<Struct>]
    type SignalType = 
        | Buy
        | Sell
        | SignalUndefined

    [<Struct>]
    type PivotType = 
        | High
        | Low
        | PivotUndefined

    [<Struct>]
    type Candle = {
        Open: decimal
        High: decimal
        Low: decimal
        Close: decimal
        Time: int64
        Volume: decimal
    }


    [<Struct>]
    type Pivot ={
        Price: decimal
        PivotType: PivotType
        Index: int
        Time: int64
    }


    [<Struct>]
    type TradeState ={
        CurrentPosition: SignalType
        Assets: decimal
        TradeCapital: decimal
        EntryPrice: decimal
    }