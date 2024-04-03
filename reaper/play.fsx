let async_fn (str: string) = async {
    let result = "Hello from " + str
    printfn "%s" result
    do! Async.Sleep 1000
    return result
}

let print_start () = printfn "Starting..."


// parse csv and extract max pric 
let get_max_price (data: string) =
    let rows = data.Split('\n')
    rows
        |> Seq.skip 1
        |> Seq.map(fun x -> x.Split(','))
        |> Seq.map(fun x -> float x.[4])
        |> Seq.max



let variance (population: float seq) = 
    let average = Seq.average population
    population
        |> Seq.map (fun x -> ((x - average) ** 2.0) / float (Seq.length population))
        |> Seq.sum



let main () =
    async {
        print_start ()
        let result = async_fn "async function" 
        print_start () 
        return 0
    }

main() |> Async.RunSynchronously |> ignore
