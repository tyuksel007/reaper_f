namespace Analytics

//todo: improvment of transition matrix, calculate time between states to exit trades
module MarkovChain =
    open System

    type Candle = { Open: float; Close: float; High: float; Low: float; Volume: float; Time: DateTime }

    let calc_pct_change (prevClose: float) (currentClose: float) : float =
        ((currentClose - prevClose) / prevClose) * 100.0

    let classify_change_by (pctChange: float) (targetChange: float) : string =
        match pctChange with
        | _ when pctChange > targetChange -> "Increase"
        | _ when pctChange < -targetChange-> "Decrease"
        | _ -> "Neutral"

    let candles_to_states (candles: Candle list) : string list =
        let targetChange = 0.01;

        let rec to_state_fn (first: Candle) (acc: string list) = function
            | [] -> []
            | head :: tail -> 
                let state = calc_pct_change first.Close head.Close 
                            |> fun change -> classify_change_by change targetChange
                to_state_fn head (state:: acc) tail

        to_state_fn candles.Head [] candles.Tail

        

    let calculate_transition_matrix (states: string list) : Map<string, Map<string, float>> =

        let update_transition (matrix: Map<string, Map<string, int>>) (prevState: string) (currentState: string) =
            let innerMap = matrix.TryFind prevState |> Option.defaultValue Map.empty
            let count = innerMap.TryFind currentState |> Option.defaultValue 0
            matrix.Add(prevState, innerMap.Add(currentState, count + 1))

        let rec loop (prevState: string) (acc: Map<string, Map<string, int>>) = function
            | [] -> acc
            | head :: tail -> 
                let updatedAcc = update_transition acc prevState head
                loop head updatedAcc tail

        let initialMatrix = Map.empty<string, Map<string, int>>
        let countMatrix = loop "Start" initialMatrix states

        let probability_matrix = 
            countMatrix 
                |> Map.map (fun _ innerMap ->
                    let total = innerMap |> Map.fold (fun acc _ value -> acc + value) 0
                    innerMap |> Map.map (fun _ value -> float value / float total))
        probability_matrix
