namespace Kucoin
#nowarn "20"

module Program =
    open Serilog
    open Microsoft.AspNetCore.Builder
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.Extensions.Hosting
    open Kucoin.Services.MarketDataService



    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        Log.Logger <- RLogger.getRLogger "https-logs.txt" "app-logs.txt"

        builder.Services.AddControllers()
        builder.Services.AddSwaggerGen()

        let app = builder.Build()

        app.UseHttpsRedirection()

        app.UseAuthorization()
        app.MapControllers()

        app.UseSwagger()
        app.UseSwaggerUI()

        app.Run()

        exitCode
