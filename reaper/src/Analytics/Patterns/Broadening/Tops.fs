namespace Analytics.Patterns

module Broadening_Tops =
    open Analytics.Types.AnalysisNs
    open System
    open Analytics.Types.MarketNs
    open FSharp.Control
    let run_analysis(contract: Contract) (interval: int) : AsyncSeq<PatternOrder array> =
        asyncSeq{
            yield [|{ 
                Symbol = "broadening_bottoms"
                Time = -1
                IntervalInMins = interval
                Pattern = "Broadening Bottoms"
                Signal = "Buy"
                EnterPrice = 100m
                TakeProfit = 200m
                StopLoss = 50m
            }|]
        }
