namespace Analytics.Patterns.Broadening

module Types = 
    open System

    type BroadeningBottomEntity = {
        Symbol: string
        IntervalInMins: int
        Time: DateTime
        PivotType: string
        PivotPrice: decimal
        ChannelLow: decimal
        ChannelHigh: decimal
        BreakoutSignal: string
        TradeCapital: decimal
    }