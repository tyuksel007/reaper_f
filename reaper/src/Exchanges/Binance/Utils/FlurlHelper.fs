namespace Binance.Utils

module FlurlHelper = 
    open Serilog
    open Flurl.Http
    open System.Text.Json
    open System

    let get_flurl_client (logger: ILogger) (enableLogging: bool): IFlurlClient =
        let flurlClient = new FlurlClient("binance url here")
        
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


    let create_signature (strToSign: string) (apiSecret: string) =
        let hmac = new Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(apiSecret))
        let bytes = System.Text.Encoding.UTF8.GetBytes(strToSign)
        let hash = hmac.ComputeHash(bytes)
        Convert.ToBase64String(hash)

    
    let sign_request (flurlRequest: IFlurlRequest) (binanceCredentials: Authorisation.BinanceCredentials)
        (method: string) (jsonBody: string option) =

        let jsonValue = match jsonBody with
                        | Some body -> body 
                        | None -> ""

        let query = if String.IsNullOrEmpty(flurlRequest.Url.Query) 
                        then "" 
                        else flurlRequest.Url.Query

        let passphraseSignature = "check docs................................................"

        let timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()

        let strToSign = (timestamp 
                                + method 
                                + Uri.UnescapeDataString(flurlRequest.Url.Path + query)
                                + jsonValue)

        let signature = create_signature strToSign binanceCredentials.ApiSecret

        flurlRequest
            .SetQueryParams([
                "timeInForce", "GTC"
                "timestamp", timestamp
                "signature", signature
            ])
            .WithHeader("X-MBX-APIKEY", binanceCredentials.ApiKey)
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Accept", "application/json")