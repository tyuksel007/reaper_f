namespace Kucoin

module TimeHelper =

    open System

    let timeFormat = "dd-MM-yyyy HH:mm"


    let toUtcEpoch (time: string) = 
        DateTime.ParseExact(time, timeFormat, System.Globalization.CultureInfo.InvariantCulture)
            .ToUniversalTime()
            .Subtract(new DateTime(1970, 1, 1))
            .TotalMilliseconds