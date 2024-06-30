namespace Analytics.Patterns.Broadening

module DbOps =
    open System.Data
    open Dapper
    open Npgsql
    open Types

    let TABLE_NAME = "broadening_bottoms"

    let private try_create_table (connection: IDbConnection) = async {
        let createQuery = $"""create table if not exists {TABLE_NAME} (Symbol varchar(50),
                IntervalInMins int,
                Time datetime, 
                PivotType varchar(50), 
                PivotPrice decimal, 
                ChannelLow decimal, 
                ChannelHigh decimal, 
                BreakoutSignal varchar(50), 
                TradeCapital decimal)
                foreign key (Symbol, IntervalInMins, Time) 
                    references {Analytics.Database.CandleOps.TABLE_NAME}(Symbol, IntervalInMins, Time)
                """
        connection.Execute(createQuery) |> ignore
        ()
    }

    let insert_broadeningDto (data: BroadeningBottomEntity) = async {
        do! Analytics.Database.Connection.execute_db_operation try_create_table 

        let save_operation (connection: IDbConnection) = async {
            let saveQuery = $"""insert into {TABLE_NAME} 
                        (Symbol, IntervalInMins, Time, PivotType, PivotPrice, ChannelLow, ChannelHigh, BreakoutSignal, TradeCapital) 
                        values 
                        (@Symbol, @IntervalInMins, @Time, @PivotType, @PivotPrice, @ChannelLow, @ChannelHigh, @BreakoutSignal, @TradeCapital)
                    """//to be continued
            connection.ExecuteAsync(saveQuery, data)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        }

        do! Analytics.Database.Connection.execute_db_operation save_operation
    }

    let read_broadeningDto () = 
        let read_operation (connection: IDbConnection) = async {
            let readQuery = $"select * from {TABLE_NAME}"
            return connection.QueryAsync<BroadeningBottomEntity>(readQuery)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> Array.ofSeq
        }

        Analytics.Database.Connection.execute_db_operation read_operation
