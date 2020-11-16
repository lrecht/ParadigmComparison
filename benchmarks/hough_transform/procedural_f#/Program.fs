// Learn more about F# at http://fsharp.org

//Include the system.drawing package
//dotnet add package System.Drawing.Common --version 5.0.0

open System
open System.Drawing

let createCosSinTables (thetaAxisSize: int) = 
    // x output ranges from 0 to pi
    // y output ranges from -maxRadius to maxRadius
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
    let maxRadius = (int)(Math.Ceiling(Math.Sqrt(Math.Pow((float)width, 2.0) + Math.Pow((float)height, 2.0))))
    let halfRAxisSize = rhoAxisSize / 2
    let outputData = Array2D.create thetaAxisSize rhoAxisSize 0
    for x in 0 .. width-1 do
        for y in 0 .. height-1 do
            let pixel = image.GetPixel(x, y)
            if pixel.Name <> "ffffffff" then
                for theta in 0..thetaAxisSize-1 do
                    let r = cosTable.[theta] * (float)x + sinTable.[theta] * (float)y;
                    let rScaled = (int) (Math.Round(r * (float)halfRAxisSize / (float)maxRadius) + (float)halfRAxisSize);
                    outputData.[theta, rScaled] <- outputData.[theta, rScaled] + 1
    
    outputData

[<EntryPoint>]
let main argv =
    let stop = System.Diagnostics.Stopwatch.StartNew()
    let image: Bitmap = new Bitmap("benchmarks/hough_transform/Pentagon.png")
    
    let thetaAxisSize = 460
    let rhoAxisSize = 360
    
    let (sinTable, cosTable) = createCosSinTables thetaAxisSize
    
    let outputData = makeHoughSpaceData cosTable sinTable image thetaAxisSize rhoAxisSize

    let newBitMap = new Bitmap(thetaAxisSize, rhoAxisSize)
    for x in 0 .. thetaAxisSize-1 do
        for y in 0 .. rhoAxisSize-1 do
            if outputData.[x, y] <= 255 then
                let num = 255-outputData.[x, y]
                newBitMap.SetPixel(x, y, Color.FromArgb(num, num, num))
            else
                newBitMap.SetPixel(x, y, Color.FromArgb(0, 0, 0)) 
    newBitMap.Save("HoughSpace.png")

    stop.Stop()
    printfn "Time: %i" stop.ElapsedMilliseconds

    0 // return an integer exit code
