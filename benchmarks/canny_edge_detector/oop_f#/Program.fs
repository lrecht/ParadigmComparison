// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Collections.Generic
open System.Drawing
open System.Linq

// Define a function to construct a message to print
let from whom =
    sprintf "from %s" whom


type Direction = Vertical = 0 | DiagonalRL = 45 | Horizontal = 90 | DiagonalLR = 135
type Colour = Black = 0 | White = 255


type ImageUtils private() =
    static let toGreyScaleColour(color : Color) =
        let level = int(float(color.R) * 0.3 + float(color.G) * 0.59 + float(color.B) * 0.11)
        Color.FromArgb(level, level, level)


    static member ToGreyScaleArray(img : Bitmap) =
        let (width, height) = img.Width, img.Height
        let mutable output = Array2D.zeroCreate width height

        if not (height > 0 || width > 0)
        then raise (Exception "Somethings not good")
            
        for x in 0 .. width - 1 do
            for y in 0 .. height - 1 do
                let greyColour = toGreyScaleColour(img.GetPixel(x, y)).R
                output.[x, y] <- int(greyColour)
        output

    static member PlotBitmap(image : Bitmap, filename) =
        image.Save(filename)

    static member PlotArray(image : int[,], filename) =
        let (width, height) = image.GetLength(0), image.GetLength(1)
        let pic = new Bitmap(width, height)
        for x in 0.. width - 1 do
            for y in 0 .. height - 1 do
                let img = image.[x,y]
                let colour =  if img > 255 then 255 else if img < 0 then 0 else img
                pic.SetPixel(x, y, Color.FromArgb(colour, colour, colour))
        ImageUtils.PlotBitmap(pic, filename)


type Convolver private() =
    static member Convolve(image : int[,], kernel : float[,]) =
        let (width, height) = image.GetLength(0), image.GetLength(1)
        let halfKernel = kernel.GetLength(0) / 2
        let mutable output : int[,] = Array2D.zeroCreate width height
        for x in 0 .. width - 1 do
            for y in 0 .. height - 1 do
                let mutable sum = 0.0
                for kernelX in -halfKernel .. halfKernel do
                    for kernelY in -halfKernel .. halfKernel do
                        let (posX, posY) = (kernelX + x, kernelY + y)
                        if posX >= 0 && posX < width && posY >= 0 && posY < height
                        then sum <- sum + (kernel.[kernelX + halfKernel, kernelY + halfKernel] * float(image.[posX, posY]))
                output.[x,y] <- int(sum)
        output


type Sobel private() =
    static let KERNEL_H = array2D [|[|-1.0; 0.0; 1.0|];[|-2.0; 0.0; 2.0|];[|-1.0; 0.0; 1.0|]|]
    static let KERNEL_V = array2D [|[|-1.0; -2.0; -1.0|]; [|0.0; 0.0; 0.0|]; [|1.0; 2.0; 1.0|]|]
    static let magnitude(image1 : int[,], image2 : int[,]) = 
        let (width, height) = image1.GetLength(0), image1.GetLength(1)
        let mutable output : int[,] = Array2D.zeroCreate width height
        let direction : Direction[,] = Array2D.zeroCreate width height
        let compassDirection = [| Direction.Vertical; Direction.DiagonalRL; Direction.Horizontal; Direction.DiagonalLR |]
        let piRad = 180.0 / Math.PI
        for x in 0 .. width - 1 do
            for y in 0 .. height - 1 do
                let colour1 = float(image1.[x, y])
                let colour2 = float(image2.[x, y])
                output.[x, y] <- int(Math.Sqrt(colour1**2.0 + colour2**2.0))
                let angle = Math.Atan2(colour1, colour2) * piRad
                let index = Math.Abs(Math.Round(angle / 45.0) % 4.0)
                direction.[x, y] <- compassDirection.[int(index)]
        (output, direction)

    static member IntensityGradient(image: int[,]) =
        let horizontalIntensity = Convolver.Convolve(image, KERNEL_H)
        let verticalIntensity = Convolver.Convolve(image, KERNEL_V)
        magnitude(horizontalIntensity, verticalIntensity)


type Gaussian private() =
    static member BlurGreyScale(image: int[,], length, intensity) =
        let (width, height) = image.GetLength(0), image.GetLength(1)
        let mutable output : int[,] = Array2D.zeroCreate width height
        let kernel = Gaussian.InitialiseKernel(length, intensity)
        output <- Convolver.Convolve(image, kernel)
        output

    static member InitialiseKernel(length, intensity) =
        let radius = length / 2
        let mutable kernel : float[,] = Array2D.zeroCreate length length
        let mutable sumTotal = 0.0
        let mutable distance = 0.0
        let calculatedEuler = 1.0 / (2.0 * Math.PI * intensity**2.0)
        for filterX in -radius .. radius do
            for filterY in -radius .. radius do
                distance <- double((filterX*filterX) + (filterY*filterY)) / (2.0 * intensity**2.0)
                kernel.[filterX + radius, filterY + radius] <- calculatedEuler * Math.Exp(-distance)
                sumTotal <- sumTotal + kernel.[filterX + radius, filterY + radius]

        for x in 0 .. length - 1 do
            for y in 0 .. length - 1 do
                kernel.[x, y] <- kernel.[x, y] * (1.0 / sumTotal)
        kernel


type Canny(filename : String) =
    let originalImage = new Bitmap(filename);
    let HIGH_THRESHOLD_VOODOO = 0.09
    let LOW_THRESHOLD_VOODOO = 0.5
    let GAUSSIAN_LENGTH = 5
    let GAUSSIAN_INTENSITY = 1.0
    let magnitudeCheck magnitude (image : int[,]) (x1, y1) (x2, y2) =
        magnitude <= image.[x1, y1] || magnitude <= image.[x2, y2]

    let nonMaxSuppression(image : int[,], direction : Direction[,]) =
        let (width, height) = (image.GetLength(0), image.GetLength(1))
        for x in 1 .. width - 2 do
            for y in 1 .. height - 2 do
                let magnitude = image.[x, y]
                let dir = direction.[x, y]
                if (dir = Direction.Vertical && magnitudeCheck magnitude image (x - 1, y)(x + 1,y)) || 
                    (dir = Direction.DiagonalRL && magnitudeCheck magnitude image (x - 1, y - 1) (x + 1, y - 1)) || 
                    (dir = Direction.Horizontal && magnitudeCheck magnitude image (x, y - 1) (x, y + 1)) || 
                    (dir = Direction.DiagonalLR && magnitudeCheck magnitude image (x + 1, y + 1) (x - 1, y - 1))
                then image.[x, y] <- int(Colour.Black)
        image

    let hasStrongNeighbour(image: int[,], thresholdHigh, x, y, width, height) =
        let mutable res = false
        for i in -1 .. 1 do
            for j in -1 .. 1 do
                let posX = x + i
                let posY = y + j
                if (i <> j && posX >= 0 && posX < width && posY >= 0 && posY < height) && float(image.[posX, posY]) > thresholdHigh
                then res <- true
        res

    let hysteresis(image : int[,], highVoodoo : double, lowVoodoo : double) =
        let (width, height) = (image.GetLength(0), image.GetLength(1))
        let arr = Seq.cast<int> image
        let mutable max = 0
        for i in arr do
            if i > max then max <- i
        let thresholdHigh = float(max) * highVoodoo
        let thresholdLow = thresholdHigh * lowVoodoo
        let output = new Bitmap(width, height)
        let weak = new List<(int * int)>()
        let mutable count = 0
        for x in 0 .. width - 1 do
            for y in 0 .. height - 1 do
                let magnitude = image.[x, y]
                if float(magnitude) >= thresholdHigh
                then
                    count <- count + 1
                    output.SetPixel(x, y, Color.White)
                else if float(magnitude) < thresholdLow
                then output.SetPixel(x, y, Color.Black)
                else weak.Add((x, y))

        for (x,y) in weak do
            let connected = hasStrongNeighbour(image, thresholdHigh, x, y, width, height)
            if connected then count <- count + 1
            let color = if connected then Color.White else Color.Black
            output.SetPixel(x, y, color)
        (output, count)

    member __.CannyEdges() =
        let mutable output = ImageUtils.ToGreyScaleArray(originalImage)
        output <- Gaussian.BlurGreyScale(output, GAUSSIAN_LENGTH, GAUSSIAN_INTENSITY)
        let mutable (output, direction) = Sobel.IntensityGradient(output)
        output <- nonMaxSuppression(output, direction)
        hysteresis(output, HIGH_THRESHOLD_VOODOO, LOW_THRESHOLD_VOODOO)


[<EntryPoint>]
let main argv =
    let (detectedEdges, whiteCount) = Canny("benchmarks/canny_edge_detector/download.jpg").CannyEdges()
    printfn "%d" whiteCount
    0 // return an integer exit code