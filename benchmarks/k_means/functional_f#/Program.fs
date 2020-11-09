let dist (x1,y1) (x2,y2) =
    System.Math.Sqrt ((x1-x2)*(x1-x2)+(y1-y2)*(y1-y2))

let closestPoint (allPoints:(float*float) array) point =
    let h = allPoints.[0]
    Array.fold (fun (shortest,s) mean -> 
                let d = dist point mean 
                if d < shortest
                then (d,mean)
                else (shortest,s)) 
              (dist point h,h) allPoints |> snd

let computeClusters clusterMeans points =
    let closePoints = Array.map (fun p -> closestPoint clusterMeans p) points
    Array.groupBy (fun i -> closePoints.[i]) [|0 .. points.Length-1|] 
    |> Array.map (fun (_,indices) -> Array.map (fun i -> points.[i]) indices)

let computeClusterMean points =
    let sumx,sumy = Array.fold (fun (xacc,yacc) (x,y) -> xacc+x,yacc+y) (0.0,0.0) points
    let len = float (Array.length points)
    sumx/len,sumy/len

let rec converge clusterMeans points =
    let newClusterMeans = Array.map computeClusterMean (computeClusters clusterMeans points)
    if clusterMeans = newClusterMeans
    then clusterMeans
    else converge newClusterMeans points

[<EntryPoint>]
let main argv =
    let points = [|(1.0,2.0);(1.2,5.4);(2.3,2.3);(5.3,6.8);(1.2,53.0);(6.5,7.8);(8.3,9.0);(5.4,6.7);(2.9,86.4);(3.5,2.4);(2.4,7.9)|]
    printfn "%A" (converge (Array.take 3 points) points)
    0 // return an integer exit code
