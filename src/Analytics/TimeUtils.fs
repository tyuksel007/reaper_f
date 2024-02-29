namespace Analytics

module TimeUtils = 
    open System

    let utcToLocalTime (time: int64) =
        let utcTime = DateTimeOffset.FromUnixTimeMilliseconds(time).DateTime.ToLocalTime()
        utcTime