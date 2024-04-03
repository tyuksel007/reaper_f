namespace Analytics.Patterns

module RLogger =
    open System.IO
    open System

    // let projectPath = "C:\\Users\\tyueksel\\Desktop\\f#\\reaper_f\\src\\Analytics"
    let projectPath = "c:/users/tyueksel/desktop/f#/reaper_f/src/analytics"

    let writeMessageToFile (nameOfCaller: string) (message: string) =
        use file = new StreamWriter(projectPath + "/patterns/logs/trade_results.txt", true)
        file.WriteLine($"{nameOfCaller}: {message}")
        file.Close()
