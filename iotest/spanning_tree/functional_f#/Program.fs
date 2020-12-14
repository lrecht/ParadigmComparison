[<EntryPoint>]
let main argv =
    let res = System.IO.File.ReadAllLines("iotest/spanning_tree/graph.csv")
    let full = Array.map (fun (s:string) -> 
        let sarr = s.Split ',' in ((int32 sarr.[0]), (int32 sarr.[1]), (int32 sarr.[2]))) res
    let c = full.Length
    printfn "%i" c
    0 // return an integer exit code
