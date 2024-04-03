namespace Binance.Utils

module Authorisation = 

    type BinanceCredentials = {
        ApiKey: string
        ApiSecret: string
    }

    let read_credentials () : BinanceCredentials =
        { ApiKey = "" ; ApiSecret = ""}