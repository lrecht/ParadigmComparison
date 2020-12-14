
[<EntryPoint>]
let main argv =
    let points = System.IO.File.ReadAllLines "iotest/kmeans/points.txt" 
                 |> Array.map (fun s -> let arr = s.Split(":") in (float arr.[0],float arr.[1]))
    let c = points.Length
    printfn "%i" c
    0 // return an integer exit code
