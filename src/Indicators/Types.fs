namespace Indicators

module Types = 

    type Candle = {
        Open: decimal
        High: decimal
        Low: decimal
        Close: decimal
        Time: int64
        Volume: decimal
    }