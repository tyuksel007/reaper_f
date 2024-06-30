namespace Analytics.Database

module Connection =
    open Npgsql

    let connString = "Host=localhost;Username=postgres;Password=123456a;Database=reaper_db"

    let initDb () =
        ()


    let execute_db_operation (operation: NpgsqlConnection -> Async<'T>) = async {
        use connection = new NpgsqlConnection(connString)
        connection.Open()
        return! operation connection
    }



