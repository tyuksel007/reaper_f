namespace Kucoin

module TimeHelper =

    open System


    let toUtcEpoch (time: string) = 
        DateTime.ParseExact(time, "dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture)
            .ToUniversalTime()
            .Subtract(new DateTime(1970, 1, 1))
            .TotalMilliseconds