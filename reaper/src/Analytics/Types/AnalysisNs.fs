namespace Analytics.Types

module AnalysisNs = 
    open System

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
    
    