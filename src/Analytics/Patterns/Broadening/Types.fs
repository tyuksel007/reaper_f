namespace Analytics.Patterns.Broadening

module Types = 
    open System

    type BroadeningBottomDto = {
        Symbol: string
        Time: DateTime
        PivotType: string
        PivotPrice: decimal
        ChannelLow: decimal
        ChannelHigh: decimal
        BreakoutSignal: string
        TradeCapital: decimal
    }