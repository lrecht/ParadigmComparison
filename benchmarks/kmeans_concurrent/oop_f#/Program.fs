// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Linq
open System.Threading.Tasks

type Point = 
    {
        X: float;
        Y: float;
    }
    static member Create (x: float) (y:float) = 
        { X = x; Y = y }
    
    member this.EuclidianDist ((cx, cy): (float*float)) = 
        Math.Sqrt(Math.Pow((cx - this.X), 2.0) + Math.Pow((cy - this.Y), 2.0));
    
    member this.ClosestCluster (clusterCentroids: (float*float) array) = 
        let mutable bestClusterIndex: int = 0
        let mutable bestDist: float = this.EuclidianDist(clusterCentroids.[0])
        for i in 1 .. clusterCentroids.Length-1 do
            let tempDist = this.EuclidianDist(clusterCentroids.[i])
            if tempDist < bestDist then
                bestDist <- tempDist
                bestClusterIndex <- i
        bestClusterIndex
    
    member this.PointsEquals (p: Point) = 
        p.X = this.X && p.Y = this.Y

    override this.ToString() = 
        this.X.ToString() + " , " + this.Y.ToString()


type Cluster = 
    {
        mutable Centroid: Point;
        mutable SumX: float;
        mutable SumY: float;
        mutable PointCount: int;
        key: Object;
    }
    static member Create (p: Point) =
        { Centroid = p; SumX = 0.0; SumY = 0.0; PointCount = 0; key = Object() }

    member this.AddToMean (p: Point) = 
        lock (this.key) (fun() ->
            this.SumX <- this.SumX + p.X
            this.SumY <- this.SumY + p.Y
            this.PointCount <- this.PointCount + 1
        )
    
    member this.ClearMeanValues() = 
        this.SumX <- 0.0;
        this.SumY <- 0.0;
        this.PointCount <- 0;

    member this.ComputeNewCentroid() = 
        let newCentroid = Point.Create (this.SumX / (float)this.PointCount) (this.SumY / (float)this.PointCount)
        let isConverged = this.Centroid.PointsEquals(newCentroid)
        this.Centroid <- newCentroid
        this.ClearMeanValues()
        isConverged
        
type KMeans = 
    {
        InitialPoints: Point array;
        Clusters: Cluster array;
    }
    static member Create (points: Point array) (kMeans: Cluster[]) = 
        { InitialPoints = points; Clusters = kMeans }

    member this.GetCentroids() =
        let centroids: (float*float) array = Array.create this.Clusters.Length (0.0, 0.0)
        for i in 0 .. this.Clusters.Length-1 do
            let centroid = this.Clusters.[i].Centroid;
            centroids.[i] <- (centroid.X, centroid.Y)
        centroids

    member this.Compute () =
        let mutable converged: bool = false;
        while not converged do
            let centroids = this.GetCentroids()
            
            Parallel.For(0, this.InitialPoints.Length, fun i -> 
            //for i in 0 .. this.InitialPoints.Length-1 do
                let point = this.InitialPoints.[i]
                let index = point.ClosestCluster(centroids)
                this.Clusters.[index].AddToMean(point)
            ) |> ignore

            converged <- true
            for i in 0 .. this.Clusters.Length - 1 do
                if (not (this.Clusters.[i].ComputeNewCentroid())) then 
                    converged <- false
            
        this.Clusters

[<EntryPoint>]
let main argv =
    let NUM_CLUSTERS: int = 10

    let lines = System.IO.File.ReadAllLines("benchmarks/kmeans_concurrent/points.txt");
    let points: Point array = Array.zeroCreate lines.Length
    for i in 0 .. lines.Length-1 do
        let split = lines.[i].Split(':')
        points.[i] <- Point.Create (Double.Parse(split.[0])) (Double.Parse(split.[1]))

    let clusters: Cluster array = Array.zeroCreate NUM_CLUSTERS
    for i in 0 .. NUM_CLUSTERS-1 do
        clusters.[i] <- Cluster.Create(points.[i])

    let kMeans = KMeans.Create points clusters
    let res = kMeans.Compute()

    for cluster in res do
        printfn "(%s)" (cluster.Centroid.ToString())

    0 // return an integer exit code


