let readFile file =
    (Seq.map (fun c -> c = '1') (System.IO.File.ReadAllText file) |> Seq.toArray)

[<EntryPoint>]
let main argv =
    let arr = readFile "iotest/game_of_life/state256.txt"
    let c = arr.Length

    printfn "%i" c

    0 // return an integer exit code
