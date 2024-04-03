namespace Kucoin.Utils

module FlurlHelper =

    open Flurl.Http
    open Serilog
    open System.Text.Json
    open Authorisation
    open System


    let getFlurlClient (logger: ILogger) (enableLogging: bool): IFlurlClient =
        let flurlClient = new FlurlClient("https://api-futures.kucoin.com")
        
        ignore (flurlClient.BeforeCall(fun call ->
            if enableLogging then
                async {
                    let requestHeaders = call.Request.Headers
                    let! streamOption = 
                        match call.Request.Content with
                        | null -> async.Return None
                        | content -> 
                            async {
                                let! stream = content.ReadAsStreamAsync() |> Async.AwaitTask
                                return Some stream
                            }
                    logger.Debug("OnBeforeCall:")
                    logger.Information("OnBeforeCall:")
                    logger.Information($"Request-Url: {call.Request.Url}")
                    logger.Information("Request-Headers: " + JsonSerializer.Serialize(requestHeaders))
                    match streamOption with
                    | Some stream -> logger.Information($"Request-Content: {stream}")
                    | None -> ()
                } |> Async.StartImmediate
        ))

        ignore (flurlClient.OnError(fun call ->
            async {
                logger.Error("OnError:")
                logger.Error($"Request-Url: {call.Request.Url}")
                logger.Error("Error: " + call.Exception.Message)
                let! response = call.Response.GetStringAsync() |> Async.AwaitTask
                logger.Error($"Response: {response}")
            } |> Async.RunSynchronously
        ))

        ignore (flurlClient.AfterCall(fun call ->
            if enableLogging then
                async {
                    logger.Information("OnAfterCall:")
                    logger.Information($"Request-Url: {call.Request.Url}")
                    let! response = call.Response.GetStringAsync() |> Async.AwaitTask
                    logger.Information($"Response: {response}")
                } |> Async.StartImmediate
        ))

        flurlClient

    
    let createSignature (strToSign: string) (apiSecret: string) =
        let hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(apiSecret))
        let bytes = System.Text.Encoding.UTF8.GetBytes(strToSign)
        let hash = hmac.ComputeHash(bytes)
        Convert.ToBase64String(hash)



    let signRequest (flurlRequest: IFlurlRequest) (kucoinCredentials: KucoinCredentials)
        (method: string) (jsonBody: string option) =

        let jsonValue = match jsonBody with
                        | Some body -> body 
                        | None -> ""

        let query = if String.IsNullOrEmpty(flurlRequest.Url.Query) 
                        then "" 
                        else flurlRequest.Url.Query

        let passphraseSignature = createSignature kucoinCredentials.ApiPassphrase kucoinCredentials.ApiSecret

        let timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()

        let strToSign = (timestamp 
                                + method 
                                + Uri.UnescapeDataString(flurlRequest.Url.Path + query)
                                + jsonValue)

        let signature = createSignature strToSign kucoinCredentials.ApiSecret

        flurlRequest.WithHeader("KC-API-KEY", kucoinCredentials.ApiKey)
                    .WithHeader("KC-API-SIGN", signature)
                    .WithHeader("KC-API-TIMESTAMP", timestamp)
                    .WithHeader("KC-API-PASSPHRASE", passphraseSignature)
                    .WithHeader("KC-API-KEY-VERSION", "2")
                    .WithHeader("Content-Type", "application/json")
                    .WithHeader("Accept", "application/json")


