let size = 256
let neighbours = List.except [(0,0)] [for x in [-1;0;1] do for y in [-1;0;1] do (x,y)]

let rec count' coorX coorY (map:Map<int*int,int>) coorList res =
    match coorList with
    | (head::tail) -> 
                let x = coorX+(fst head)
                let y = coorY+(snd head)
                if x >= 1 && x <= size && y >= 1 && y <= size then
                    count' coorX coorY map tail (res+(map.[(x,y)]))
                else count' coorX coorY map tail res
    | _ -> res

let count (coorX,coorY) map =
    count' coorX coorY map neighbours 0

let rules cell count =
    match cell,count with
    | 1,(2|3) -> 1
    | 0,3 -> 1
    | _ -> 0

let updateCells (map:Map<int*int,int>) =
    Map.fold (fun accMap coor cell -> 
                Map.add coor (rules cell (count coor map)) accMap) Map.empty map

[<EntryPoint>]
let main argv =
    let rand = System.Random()
    let assocList = [ for x in [1..size] do for y in [1..size] do ((x,y),rand.Next(0,2)) ]
    let map = Map.ofList assocList

    printfn "%A"  map
    printfn "%A" (updateCells map)
    0 // return an integer exit code