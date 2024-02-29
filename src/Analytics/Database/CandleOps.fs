namespace Analytics.Database

module CandleOps =
    open Analytics.Types
    open System.Data.SQLite
    open Dapper
    open System

    type CandleDto = { 
            Symbol: string
            Time: DateTime
            Close: decimal
            Open: decimal
            High: decimal
            Low: decimal
        }

    let tryCreateSymbolTable (connection: SQLiteConnection) = 
        let createTableQuery = "create table if not exists symbols (Symbol varchar(50), Time datetime, Close decimal, Open decimal, High decimal, Low decimal)"
        use command = new SQLiteCommand(createTableQuery, connection)
        command.ExecuteNonQuery() |> ignore
        ()


    let save_candles (connection: SQLiteConnection) (symbol: string) (candles: Candle array) = 

        tryCreateSymbolTable connection 

        let insertQuery = "insert into symbols (Symbol, Time, Close, Open, High, Low) values (@Time, @Close, @Open, @High, @Low)" 
        try

            candles
                |> Array.iter (fun candle -> 

                    let candleDto: CandleDto = { 
                        Symbol = symbol
                        Time = Analytics.TimeUtils.utcToLocalTime candle.Time
                        Close = candle.Close
                        Open = candle.Open
                        High = candle.High
                        Low = candle.Low
                    } 

                    connection.Execute(insertQuery, candleDto) |> ignore
                )
        with   
            | ex -> () 

    
