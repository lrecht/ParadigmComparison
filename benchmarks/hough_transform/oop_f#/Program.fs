// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Drawing


type Maths =
    static member Hypotenuse width height = Math.Sqrt(float(width*width + height*height))
    static member CosSinRadianTable thetaAxisSize =
        let mutable sinTable : double array = Array.zeroCreate thetaAxisSize
        let mutable cosTable : double array = Array.zeroCreate thetaAxisSize
        for theta in 0 .. thetaAxisSize - 1 do
            let thetaRadians = (float)theta * Math.PI / (float)thetaAxisSize
            sinTable.[theta] <- Math.Sin(thetaRadians)
            cosTable.[theta] <- Math.Cos(thetaRadians)
        (sinTable, cosTable)


type PictureData(filename:String) =
    let original = new Bitmap(filename)
    member __.Width = original.Width
    member __.Height = original.Height
    member __.GetPixelColor x y = original.GetPixel(x, y)


type HoughTransform(filename, rhoAxisSize, thetaAxisSize) =
    let mutable transformed : int[,] = Array2D.zeroCreate thetaAxisSize rhoAxisSize
    let mutable pictureData = PictureData(filename)
    member __.ComputeTransformation() =
        let width = pictureData.Width
        let height = pictureData.Height
        let diagonal = Math.Ceiling(Maths.Hypotenuse width height)
        let halfRhoAxisSize = (float)rhoAxisSize / 2.0
        let (sinTable, cosTable) = Maths.CosSinRadianTable thetaAxisSize
        for y in 0 .. height - 1 do
            for x in 0 .. width - 1 do
                let color = pictureData.GetPixelColor x y
                if color.Name <> "ffffffff"
                then
                    for theta in 0 .. thetaAxisSize - 1 do
                        let rho = cosTable.[theta] * (float)x + sinTable.[theta] * (float)y
                        let rScaled = int(Math.Round(rho * halfRhoAxisSize / diagonal) + halfRhoAxisSize)
                        transformed.[theta, rScaled] <- transformed.[theta, rScaled] + 1
        transformed


[<EntryPoint>]
let main argv =
    let hough = HoughTransform("benchmarks/hough_transform/Pentagon.png", 480, 640)
    let output = hough.ComputeTransformation()
    let mutable sum = 0
    for cell in Seq.cast<int> output do
        sum <- sum + cell
    printfn "%A" sum
    0 // return an integer exit code