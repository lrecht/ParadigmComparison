﻿open System
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

let incrementAcc (x,y) (accumulator:Map<int*int,int>) = 
    Array.fold (fun (acc:Map<int*int,int>) theta ->
        let rho = round (x * cosValues.[theta] + y * sinValues.[theta])
        let scaleRho = int (Math.Round ((rho * halfRhoSize / float diagonal) + halfRhoSize))
        let num = if acc.ContainsKey (theta,scaleRho) then acc.[theta,scaleRho] + 1 else 1
        acc.Add ((theta,scaleRho),num)) accumulator thetaRange

let hough (image:Bitmap) =
    let accumulator = Map.empty
    [|for x in [0 .. width-1] do for y in [0 .. height-1] do (x,y)|]
    |> Array.fold (fun acc (x,y) -> if image.GetPixel(x,y).Name = "ffffffff"
                                    then acc 
                                    else (incrementAcc (float x,float y) acc)) accumulator 

[<EntryPoint>]
let main argv =
    let h = hough bm
    printfn "%i" (Map.fold (fun acc k v -> acc+v) 0 h)
    0 // return an integer exit code
