// Learn more about F# at http://fsharp.org

//Include the system.drawing package
//dotnet add package System.Drawing.Common --version 5.0.0

open System
open System.Drawing
open benchmark

let createCosSinTables (thetaAxisSize: int) = 
    let sinTable = Array.create thetaAxisSize 0.0
    let cosTable = Array.create thetaAxisSize 0.0
    for theta in 0 .. thetaAxisSize-1 do
        let thetaRadians = (float)theta * Math.PI / (float)thetaAxisSize
        sinTable.[theta] <- Math.Sin(thetaRadians)
        cosTable.[theta] <- Math.Cos(thetaRadians)
    (sinTable, cosTable)

let makeHoughSpaceData (cosTable: float[]) (sinTable: float[]) (image: Bitmap) (thetaAxisSize: int) (rhoAxisSize: int) = 
    let width = image.Width
    let height = image.Height
    let diagonal = (int)(Math.Ceiling(Math.Sqrt(Math.Pow((float)width, 2.0) + Math.Pow((float)height, 2.0)))) //Max radius
    let halfRAxisSize = rhoAxisSize / 2
    let outputData = Array2D.create thetaAxisSize rhoAxisSize 0
    for x in 0 .. width-1 do
        for y in 0 .. height-1 do
            let pixel = image.GetPixel(x, y)
            if pixel.Name <> "ffffffff" then
                for theta in 0..thetaAxisSize-1 do
                    let r = cosTable.[theta] * (float)x + sinTable.[theta] * (float)y;
                    let rScaled = (int) (Math.Round(r * (float)halfRAxisSize / (float)diagonal) + (float)halfRAxisSize);
                    outputData.[theta, rScaled] <- outputData.[theta, rScaled] + 1
    outputData

[<EntryPoint>]
let main argv =
    let iterations = if argv.Length > 0 then int (argv.[0]) else 1
    let bm = Benchmark(iterations)

    let image: Bitmap = new Bitmap("benchmarks/hough_transform/Pentagon.png")
    
    bm.Run((fun () ->
        let thetaAxisSize = 640
        let rhoAxisSize = 480
        
        let (sinTable, cosTable) = createCosSinTables thetaAxisSize
        
        let outputData = makeHoughSpaceData cosTable sinTable image thetaAxisSize rhoAxisSize

        let mutable sum = 0
        for x in 0 .. outputData.GetLength(0)-1 do 
            for y in 0 .. outputData.GetLength(1)-1 do
                sum <- sum + outputData.[x, y]
        sum
    ), (fun (res) ->
        printfn "Sum: %i" res
    ))

    0 // return an integer exit code
