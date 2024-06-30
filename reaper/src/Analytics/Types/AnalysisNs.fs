namespace Analytics.Types

module AnalysisNs = 
    open System
    open MarketNs

    [<Struct>]
    type PivotType = 
        | High
        | Low
        | PivotUndefined


    [<Struct>]
    type Pivot ={
        Price: decimal
        PivotType: PivotType
        Index: int
        Time: int64
    }

    [<Struct>]
    type RegressionValues = {
        Slope: float
        Intercept: float
        RSquared: float
    }

    type PatternOrder = {
        Symbol: string
        Time: int64
        IntervalInMins: int
        Pattern: string
        Signal: string
        EnterPrice: decimal
        TakeProfit: decimal
        StopLoss: decimal
    }
    
    type AnalysisResult = {
        Order: PatternOrder 
        LowerLine: RegressionValues
        UpperLine: RegressionValues
        LowPoints: Candle array
        HighPoints: Candle array
    }
    