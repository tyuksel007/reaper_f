namespace Analytics.Patterns.Broadening

module DbOps =
    open System.Data.SQLite
    open Dapper
    open Types

    let try_create_table (connection: SQLiteConnection) = 
        let createQuery = "create table if not exists broadening_bottoms (Symbol varchar(50), Time datetime, PivotType varchar(50), PivotPrice decimal, ChannelLow decimal, ChannelHigh decimal, BreakoutSignal varchar(50), TradeCapital decimal)"
        connection.Execute(createQuery) |> ignore
        ()

    let insert_broadeningDto (connection: SQLiteConnection)  (data: BroadeningBottomDto) = 
        let saveQuery = "insert into broadening_bottoms (Symbol, Time, PivotType, PivotPrice, ChannelLow, ChannelHigh, BreakoutSignal, TradeCapital) values (@Symbol, @Time, @PivotType, @PivotPrice, @ChannelLow, @ChannelHigh, @BreakoutSignal, @TradeCapital)"
        connection.Execute(saveQuery, data) |> ignore
        ()

    let read_broadeningDto (connection: SQLiteConnection) = 
        let readQuery = "select * from broadening_bottoms"
        connection.Query<BroadeningBottomDto>(readQuery) |> Seq.toArray 
