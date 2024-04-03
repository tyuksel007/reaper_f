namespace Kucoin.Types

module MarketDataApiNs = 

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


    type ContractApiResponse = {
        Code: string
        Data: List<Contract>
    }


    type CandlesApiResponse = {
        Code: string
        Data: List<List<decimal>>
    }


    type SymbolDetail = {
        Code: string
        Data: Contract
    }
