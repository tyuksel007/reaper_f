namespace Analytics.Database

module CandleOps =
    open Analytics.Types.MarketNs
    open Dapper
    open Npgsql


    type CandleEntity = { 
            Symbol: string
            IntervalInMins: int
            Time: int64
            Close: decimal
            Open: decimal
            High: decimal
            Low: decimal
            Volume: decimal
        }
    
    let TABLE_NAME = "candles"


    let private tryCreateCandleTable (connection: NpgsqlConnection) = 
        try
            let createTableQuery = $"""create table if not exists {TABLE_NAME}
                (Symbol varchar(50), IntervalInMins int, Time bigint,
                Close decimal, Open decimal, 
                High decimal, Low decimal, Volume decimal,
                primary key(Symbol, IntervalInMins, Time))"""

            use command = new NpgsqlCommand(createTableQuery, connection)
            command.ExecuteNonQuery() |> ignore
        with
            | ex -> printfn "Error while creating candles table............: %s" ex.Message
        ()


    let save_candles (symbol: string) (intervalInMins: int) (candles: Candle array) = 

        Connection.execute_db_operation tryCreateCandleTable 

        let save_operation (connection: NpgsqlConnection)= 
            let insertQuery = $"""INSERT INTO {TABLE_NAME} 
                    (Symbol, IntervalInMins, Time, Close, Open, High, Low, Volume) 
                    values 
                    (@Symbol, @IntervalInMins, @Time, @Close, @Open, @High, @Low, @Volume)
                    on conflict do nothing
                    """

            try
                candles
                    |> Array.iter (fun candle -> 
                        let candle_to_save: CandleEntity = { 
                            Symbol = symbol
                            IntervalInMins = intervalInMins
                            Time = candle.Time
                            Close = candle.Close
                            Open = candle.Open
                            High = candle.High
                            Low = candle.Low
                            Volume = candle.Volume
                        } 
                        connection.Execute(insertQuery, candle_to_save) |> ignore
                    )
            with   
                | ex -> printfn "Error while saving candles............: %s" ex.Message

        Connection.execute_db_operation save_operation
    



    let read_candles (symbol: string) (intervalInMins: int) (from: int64) (toDate: int64) = 
        try
            let read_operation (connection: NpgsqlConnection) =
                let selectQuery = $"""select * from {TABLE_NAME} 
                                    where Symbol = @Symbol 
                                    and IntervalInMins = @IntervalInMins
                                    and Time >= @From 
                                    and Time <= @To"""
                let candles = connection.Query<CandleEntity>(selectQuery, 
                    {| Symbol = symbol; IntervalInMins = intervalInMins; From = from; To = toDate |})
                candles

            Connection.execute_db_operation read_operation
        with
            | ex -> 
                printfn "Error while reading candles............: %s" ex.Message 
                [||]


