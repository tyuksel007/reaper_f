namespace Analytics

module TimeUtils = 
    open System

    let timeFormat = "dd-MM-yyyy HH:mm"

    let utcToLocalTime (time: int64) =
        let utcTime = DateTimeOffset.FromUnixTimeMilliseconds(time).DateTime.ToLocalTime()
        utcTime

    let toUtcEpoch (time: string) = 
        int64 (DateTime.ParseExact(time, timeFormat, System.Globalization.CultureInfo.InvariantCulture)
            .ToUniversalTime()
            .Subtract(new DateTime(1970, 1, 1))
            .TotalMilliseconds)