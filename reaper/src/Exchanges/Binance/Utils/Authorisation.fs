namespace Binance.Utils

module Authorisation = 
    open System.IO
    open System.Text.Json

    type BinanceCredentials = {
        ApiKey: string
        ApiSecret: string
    }

    type Credentials = {
        Binance: BinanceCredentials
    }

    let read_credentials () : BinanceCredentials =
        let json = File.ReadAllText("C:\\Users\\tyueksel\\Desktop\\f#\\reaper_f\\reaper\\secrets.json")
        JsonSerializer.Deserialize<Credentials>(json).Binance