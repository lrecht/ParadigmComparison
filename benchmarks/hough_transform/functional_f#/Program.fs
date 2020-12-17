open System
open System.Drawing

let bm = new Bitmap("benchmarks/hough_transform/Pentagon.png")
let width = bm.Width
let height = bm.Height
let diagonal = Math.Sqrt (float ((width*width)+(height*height))) |> int
let thetaSize = 640
let rhoSize = 480
let halfRhoSize = float (rhoSize / 2)
let thetaRange = [|0 .. thetaSize-1|]
let cosValues = Array.map (fun t -> Math.Cos ((float t)*Math.PI/float thetaSize)) thetaRange
let sinValues = Array.map (fun t -> Math.Sin ((float t)*Math.PI/float thetaSize)) thetaRange

let getLines (x,y) = 
    Array.map (fun theta ->
        let rho = round (float x * cosValues.[theta] + float y * sinValues.[theta])
        let scaleRho = int (Math.Round ((rho * halfRhoSize / float diagonal) + halfRhoSize))
        theta,scaleRho) thetaRange

let hough (image:Bitmap) =
    [|for x in [0 .. width-1] do for y in [0 .. height-1] do (x,y)|]
    |> Array.filter (fun (x,y) -> image.GetPixel(x,y).Name <> "ffffffff")
    |> Array.collect getLines
    |> Array.groupBy id
    |> Array.map (fun (k,list) -> k,(Array.length list))

[<EntryPoint>]
let main argv =
    let h = hough bm
    printfn "%i" (Array.fold (fun acc (k,v) -> acc+v) 0 h)
    0 // return an integer exit code
