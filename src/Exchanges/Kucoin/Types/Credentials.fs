namespace Kucoin

module Credentials = 

    open System.IO
    open System.Text.Json

    type KucoinCredentials = {
        ApiKey: string
        ApiSecret: string
        ApiPassphrase: string
    }

    let readCredentials() : KucoinCredentials =
        let json = File.ReadAllText("C:\\Users\\tyueksel\\Desktop\\f#\\Qconnect\\secrets.json")
        JsonSerializer.Deserialize<KucoinCredentials>(json)

