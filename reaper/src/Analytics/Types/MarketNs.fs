namespace Analytics.Types

module MarketNs = 

    [<Struct>]
    type Contract = {
        Symbol: string
        MarkPrice: decimal
    }


    [<Struct>]
    type Candle = {
        Open: decimal
        High: decimal
        Low: decimal
        Close: decimal
        Time: int64
        Volume: decimal
    }