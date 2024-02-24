namespace Kucoin.Types

type Contract = {
    Symbol: string
    MarkPrice: decimal
    MakerFeeRate: decimal
    TakerFeeRate: decimal
    HighPrice: decimal
    LowPrice: decimal
    PriceChg: decimal
    PriceChgPct: decimal
}

type ContractApiResponse = {
    Code: string
    Data: List<Contract>
}


type Kline = {
    Open: decimal
    High: decimal
    Low: decimal
    Close: decimal
    Time: int64
    Volume: decimal
}

type KlinesApiResponse = {
    Code: string
    Data: List<List<decimal>>
}



type SymbolData = {
    Symbol: string
    MarkPrice: decimal
    MakerFeeRate: decimal
    TakerFeeRate: decimal
    HighPrice: decimal
    LowPrice: decimal
    PriceChg: decimal
    PriceChgPct: decimal
}

type SymbolDetail = {
    Code: string
    Data: SymbolData
}