namespace Analytics.Patterns

module Head_Shoulders_DownTrend = 
    open MathNet.Numerics
    open System
    open Analytics.Types.MarketNs
    open Analytics.Types.TradeNs
    open Analytics.Types.AnalysisNs
    open Analytics


    let rec get_length_of_list l = function
        | [] -> l
        | _::tail -> get_length_of_list (l+1) tail

    let rec get_second_last_element = function
        | head::tail::[] -> head
        | _::tail -> get_second_last_element tail
        | _ -> failwith "List is too short" 

    let foo () =
        let range_arr  = 10 :: [1..10]
        let setr = Set.ofList range_arr
        let mySeq = 
            Seq.unfold 
                (fun state -> if state > 10 then None else Some (state, state + 1))
                1
        ()



    let rec fib = function
        | 1 | 2 -> 1
        | n -> fib (n-1) + fib (n-2)

    let rec sum = function
        | [] -> 0
        | head::tail -> head + sum tail

    let rec mymap f = function
        | [] -> []
        | head::tail -> f head :: mymap f tail

    let rec fac = function
        | 0 | 1 -> 1
        | n -> n * fac (n-1)

