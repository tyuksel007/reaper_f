namespace Binance.Types

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


    type BinanceCandle = {
        OpenTime: int64
        OpenPrice: decimal
        HighPrice: decimal
        LowPrice: decimal
        ClosePrice: decimal
        Volume: decimal
        CloseTime: int64
        QuoteAssetVolume: decimal
        NumberOfTrades: int
        TakerBuyBaseAssetVolume: decimal
        TakerBuyQuoteAssetVolume: decimal
    }


    type SymbolDetail = {
        Code: string
        Data: Contract
    }
