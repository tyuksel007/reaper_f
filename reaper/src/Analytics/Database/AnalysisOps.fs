namespace Analytics.Database

module AnalysisOps =
    open Analytics.Types.AnalysisNs
    open FSharp.Control
    open Npgsql
    open Dapper

    let read_analysis(symbol: string) (intervalInMins: int) (from: int64 option) (toDate: int64 option) = asyncSeq {
        try
            let read_operation (connection: NpgsqlConnection) = async {
                let select_query = 
                    match from, toDate with
                    | Some f, None ->
                        $"""select * from analysis where Symbol = '{symbol}' and IntervalInMins = {intervalInMins} and Time >= {f}"""
                    | None, Some t ->
                        $"""select * from analysis where Symbol = '{symbol}' and IntervalInMins = {intervalInMins} and Time <= {t}"""
                    | Some f, Some t ->
                        $"""select * from analysis where Symbol = '{symbol}' and IntervalInMins = {intervalInMins} and Time >= {f} and Time <= {t}"""
                    | _ ->
                        $"""select * from analysis where Symbol = '{symbol}' and IntervalInMins = {intervalInMins}"""

                return connection.QueryAsync<AnalysisResult>(select_query,
                    {| Symbol = symbol; IntervalInMins = intervalInMins; From = from; To = toDate |}) 
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> Array.ofSeq
            }

            let! analysis_res =  Connection.execute_db_operation read_operation
            yield analysis_res
                
        with
            | ex -> printfn "Error while reading analysis............: %s" ex.Message
                    yield [||]
    }

    let save_analysis (analysis: AnalysisResult) = async {
        failwith "not implemented"
    }