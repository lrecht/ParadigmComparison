﻿let dist (x1,y1) (x2,y2) =
    System.Math.Sqrt ((x1-x2)*(x1-x2)+(y1-y2)*(y1-y2))

let closestPoint (allPoints:(float*float) array) point =
    Array.fold (fun (shortest,s) mean -> 
                let d = dist point mean 
                if d < shortest then (d,mean) else (shortest,s)) 
              (dist point allPoints.[0],allPoints.[0]) allPoints |> snd

let computeClusters clusterMeans points =
    Array.groupBy (fun p -> closestPoint clusterMeans p) points
    |> Array.map (snd >> 
                 (fun points -> Array.averageBy fst points, Array.averageBy snd points))

let rec converge clusterMeans points =
    let newClusterMeans = computeClusters clusterMeans points
    if clusterMeans = newClusterMeans then clusterMeans else converge newClusterMeans points

[<EntryPoint>]
let main argv =
    let points = System.IO.File.ReadAllLines "benchmarks/kmeans/points.txt" 
                 |> Array.map (fun s -> let arr = s.Split(":") in (float arr.[0],float arr.[1]))
    let clusters = 10
    printfn "%A" (converge (Array.take clusters points) points)
    0 // return an integer exit code