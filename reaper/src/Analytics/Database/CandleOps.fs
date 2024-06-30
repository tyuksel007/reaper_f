namespace Analytics.Database

module CandleOps =
    open Analytics.Types.MarketNs
    open Dapper
    open Npgsql
    open FSharp.Control


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


    let private tryCreateCandleTable (connection: NpgsqlConnection) = async {
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
    }


    let save_candles (symbol: string) (intervalInMins: int) (candles: Candle array) = async {

        do! Connection.execute_db_operation tryCreateCandleTable 

        let save_operation (connection: NpgsqlConnection) = async { 
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
                        connection.ExecuteAsync(insertQuery, candle_to_save) 
                        |> Async.AwaitTask 
                        |> Async.RunSynchronously 
                        |> ignore
                    )
            with   
                | ex -> printfn "Error while saving candles............: %s" ex.Message
        }

        do! Connection.execute_db_operation save_operation
    }
    



    let read_candles (symbol: string) (interval_in_mins: int) (from: int64 option) (toDate: int64 option) = asyncSeq {
        try
            let read_operation (symbol_: string) (interval: int) (from_: int64) (to_: int64) (connection: NpgsqlConnection) = async {
                let selectQuery = 
                    match (from_ , to_) with
                    | (f , t) when f > 0 && t > 0 -> 
                        $"""select * from {TABLE_NAME} 
                                where Symbol = @Symbol 
                                and IntervalInMins = @IntervalInMins
                                and Time >= @From 
                                and Time <= @To"""
                    | (f, t) when f > 0 && t <= 0 ->
                        $"""select * from {TABLE_NAME} 
                                where Symbol = @Symbol 
                                and IntervalInMins = @IntervalInMins
                                and Time >= @From"""
                    | (f, t) when f <= 0 && t > 0 ->
                        $"""select * from {TABLE_NAME} 
                                where Symbol = @Symbol 
                                and IntervalInMins = @IntervalInMins
                                and Time <= @To"""
                    | _ ->
                        $"""select * from {TABLE_NAME} 
                                where Symbol = @Symbol 
                                and IntervalInMins = @IntervalInMins"""
                let candles =
                    connection.QueryAsync<CandleEntity>(selectQuery, 
                        {| Symbol = symbol_; IntervalInMins = interval; From = from_; To = to_ |})
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                    |> Array.ofSeq
                return candles
            }


            
            let mutable start = 
                match from with
                | Some f -> float f
                | None -> -1
            let to_ = 
                match toDate with
                | Some t -> float t
                | None -> -1

            let chunk_size = 200
            let delta_in_minutes = (to_ - start) / 1000.0 / 60.0
            let total_num_of_candles = delta_in_minutes / float interval_in_mins
            let count_of_iteration = int (ceil total_num_of_candles / float chunk_size)
            
            for i = 0 to count_of_iteration do
                let next_to = start + (float chunk_size * float interval_in_mins) * 1000.0 * 60.0
                let next= min next_to to_
                
                let read = read_operation symbol interval_in_mins (int64 start) (int64 next)

                start <- next_to

                let! res = Connection.execute_db_operation read
                yield res
                    |> Array.map (fun c ->
                    { Time = c.Time; Close = c.Close; Open = c.Open; High = c.High; Low = c.Low; Volume = c.Volume }) 
        with
            | ex -> 
                printfn "Error while reading candles............: %s" ex.Message 
                yield [||]
    }


