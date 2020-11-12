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
    Array.Parallel.map (fun p -> closestPoint clusterMeans p,p) points 
    |> Array.groupBy fst
    |> Array.map (fun (_,list) -> Array.map snd list)

let computeClusterMean points =
    Array.averageBy fst points, Array.averageBy snd points

let rec converge clusterMeans points =
    let newClusterMeans = Array.map computeClusterMean (computeClusters clusterMeans points)
    if clusterMeans = newClusterMeans
    then clusterMeans
    else converge newClusterMeans points

[<EntryPoint>]
let main argv =
    let points = System.IO.File.ReadAllLines "benchmarks/kmeans_concurrent/points.txt" 
                 |> Array.map (fun s -> let arr = s.Split(":") in (float arr.[0],float arr.[1]))
    let clusters = 10
    printfn "%A" (converge (Array.take clusters points) points)
    0 // return an integer exit code
