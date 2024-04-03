namespace Kucoin.Utils

module RLogger =

    open Serilog

    let get_default_rlogger (): Serilog.Core.Logger = 
        let httpsLogsPath = "logs/https.log"
        let appLogsPath = "logs/app.log"
        LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Logger(fun lc -> 
                (lc.Filter.ByIncludingOnly(fun le -> le.Properties.ContainsKey("http"))
                    .WriteTo.File(httpsLogsPath, rollingInterval =  RollingInterval.Hour, outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")) |> ignore)
            .WriteTo.Logger(fun lc -> 
                (lc.Filter.ByIncludingOnly(fun le -> le.Properties.ContainsKey("app"))
                    .WriteTo.File(appLogsPath, rollingInterval = RollingInterval.Hour, outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")) |> ignore)
            .CreateLogger()

    let get_r_logger (httpsLogsPath: string) (appLogsPath: string) : Serilog.Core.Logger = 
        LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Logger(fun lc -> 
                (lc.Filter.ByIncludingOnly(fun le -> le.Properties.ContainsKey("http"))
                    .WriteTo.File(httpsLogsPath, rollingInterval =  RollingInterval.Hour, outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")) |> ignore)
            .WriteTo.Logger(fun lc -> 
                (lc.Filter.ByIncludingOnly(fun le -> le.Properties.ContainsKey("app"))
                    .WriteTo.File(appLogsPath, rollingInterval = RollingInterval.Hour, outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")) |> ignore)
            .CreateLogger()


    let httpLogger = Log.ForContext("http", "")
    let appLogger = Log.ForContext("app", "")

