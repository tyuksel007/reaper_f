namespace Analytics.Database

module Connection =

    open System
    open System.Data.SQLite


    let databaseFilename = "analytics.sqlite"
    let connectionStringFile = sprintf "Data Source=%s;Version=3;" databaseFilename  

    let initDb () =
        if not (IO.File.Exists(databaseFilename)) then
            SQLiteConnection.CreateFile(databaseFilename)




