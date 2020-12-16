// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Linq
open System.Threading.Tasks
open benchmark

type Point(x, y) = 
    member __.X = x
    member __.Y = y
    member __.EuclidianDist ((cx, cy): (float*float)) = 
        Math.Sqrt(Math.Pow((cx - x), 2.0) + Math.Pow((cy - y), 2.0));
    
    member this.ClosestCluster (clusterCentroids: (float*float) array) = 
        let mutable bestClusterIndex: int = 0
        let mutable bestDist: float = this.EuclidianDist(clusterCentroids.[0])
        for i in 1 .. clusterCentroids.Length-1 do
            let tempDist = this.EuclidianDist(clusterCentroids.[i])
            if tempDist < bestDist then
                bestDist <- tempDist
                bestClusterIndex <- i
        bestClusterIndex
    
    member ___.PointsEquals (p: Point) = 
        p.X = x && p.Y = y

    override __.ToString() = 
        x.ToString() + " , " + y.ToString()


type Cluster(cent: Point) as this =
    let mutable sumX = 0.0
    let mutable sumY = 0.0
    let mutable pointCount = 0
    let mutable key = Object()
    member val Centroid = cent with get,set
    member __.AddToMean (p: Point) = 
        lock (key) (fun() ->
            sumX <- sumX + p.X
            sumY <- sumY + p.Y
            pointCount <- pointCount + 1
        )
    
    member __.ClearMeanValues() = 
        sumX <- 0.0;
        sumY <- 0.0;
        pointCount <- 0;

    member __.ComputeNewCentroid() = 
        let newCentroid = Point(sumX / (float)pointCount, sumY / (float)pointCount)
        let isConverged = this.Centroid.PointsEquals(newCentroid)
        this.Centroid <- newCentroid
        this.ClearMeanValues()
        isConverged
        
type KMeans(initialPoints: Point array, clusters: Cluster[]) = 
    member __.GetCentroids() =
        let centroids: (float*float) array = Array.create clusters.Length (0.0, 0.0)
        for i in 0 .. clusters.Length-1 do
            let centroid = clusters.[i].Centroid;
            centroids.[i] <- (centroid.X, centroid.Y)
        centroids

    member this.Compute () =
        let mutable converged: bool = false;
        while not converged do
            let centroids = this.GetCentroids()
            
            Parallel.For(0, initialPoints.Length, fun i -> 
                let point = initialPoints.[i]
                let index = point.ClosestCluster(centroids)
                clusters.[index].AddToMean(point)
            ) |> ignore

            converged <- true
            for i in 0 .. clusters.Length - 1 do
                if (not (clusters.[i].ComputeNewCentroid())) then 
                    converged <- false
        clusters


[<Literal>]
let NUM_CLUSTERS: int = 10


[<EntryPoint>]
let main argv =
    let iterations = if argv.Length > 0 then int (argv.[0]) else 1
    let bm = Benchmark(iterations)

    let lines = System.IO.File.ReadAllLines("benchmarks/kmeans_concurrent/points.txt");
    let points: Point array = Array.zeroCreate lines.Length
    for i in 0 .. lines.Length-1 do
        let split = lines.[i].Split(':')
        points.[i] <- Point(Double.Parse(split.[0]), (Double.Parse(split.[1])))

    bm.Run((fun () ->
        let clusters: Cluster array = Array.zeroCreate NUM_CLUSTERS
        for i in 0 .. NUM_CLUSTERS-1 do
            clusters.[i] <- Cluster(points.[i])

        let kMeans = KMeans(points, clusters)
        kMeans.Compute()
    ), (fun (res) ->
        for cluster in res do
            printfn "(%s)" (cluster.Centroid.ToString())
    ))
    
    0 // return an integer exit code

