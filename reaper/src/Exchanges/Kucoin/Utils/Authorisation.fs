namespace Kucoin.Utils

module Authorisation = 

    open System.IO
    open System.Text.Json

    type KucoinCredentials = {
        ApiKey: string
        ApiSecret: string
        ApiPassphrase: string
    }

    type private Credentials = {
        Kucoin: KucoinCredentials
    }


    let readCredentials() : KucoinCredentials =
        let json = File.ReadAllText("C:\\Users\\tyueksel\\Desktop\\f#\\reaper_f\\reaper\\secrets.json")
        JsonSerializer.Deserialize<Credentials>(json).Kucoin

