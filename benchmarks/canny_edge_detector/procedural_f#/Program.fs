// Learn more about F# at http://fsharp.org

open System
open System.Drawing

let weak = 100
let black = 0
let white = 255
let getPixel (image: Bitmap) (x: int) (y: int) = (int)(image.GetPixel(x, y).R)

let kernelHor = array2D [
    [-1.0; 0.0; 1.0]
    [-2.0; 0.0; 2.0]
    [-1.0; 0.0; 1.0]]

let kernelVer = array2D [
    [1.0; 2.0; 1.0]
    [0.0; 0.0; 0.0]
    [-1.0; -2.0; -1.0]]


// https://epochabuse.com/gaussian-blur/
let GaussianFilter (length: int) (weight: float) = 
    let kernel = Array2D.create length length 0.0
    let mutable kernelSum = 0.0
    let foff = (length - 1) / 2
    let mutable distance = 0.0
    let constant = 1.0 / (2.0 * Math.PI * weight * weight)
    for y in -foff .. foff do
        for x in -foff .. foff do
            distance <- (float)((y * y) + (x * x)) / (2.0 * (weight * weight))
            kernel.[y + foff, x + foff] <- constant * Math.Exp(-distance)
            kernelSum <- kernelSum + kernel.[y + foff, x + foff]

    for y in 0 .. length-1 do
        for x in 0 .. length-1 do
            kernel.[y, x] <- kernel.[y, x] * 1.0 / kernelSum
    kernel

//My own slow Convolve
let Convolve (image: int[,]) (filter: float[,]) =
    let width = image.GetLength(0)
    let height = image.GetLength(1)
    //Kernel has to be an odd number
    let halfKernel = filter.GetLength(0) / 2
    
    let test: int[,] = Array2D.zeroCreate width height
    for x in 0 .. width-1 do
        for y in 0 .. height-1 do
            
            let mutable sum = 0.0
            for kx in -halfKernel .. halfKernel do
                for ky in -halfKernel .. halfKernel do
                    let posX = x + kx
                    let posY = y + ky
                    // not edges
                    if not (posX < 0 || posX > width-1 || posY < 0 || posY > height - 1) then
                        sum <- sum + ((float)(image.[posX, posY]) * (filter.[kx+halfKernel, ky+halfKernel]))
            
            test.[x, y] <- (int) sum
    test

let hyp (num1: int) (num2: int) = 
    (int) (Math.Sqrt((float)(num1 * num1) + (float)(num2 * num2)))

let computeIntensity (image: int[,]) = 
    let Ix = Convolve image kernelHor
    let Iy = Convolve image kernelVer
    
    let g: int[,] = Array2D.zeroCreate (image.GetLength(0)) (image.GetLength(1))
    let thetaWidth = Iy.GetLength(0)
    let thetaHeight = Iy.GetLength(1)
    let thetaQ = Array2D.zeroCreate thetaWidth thetaHeight
    for x in 0 .. thetaWidth-1 do
        for y in 0 .. thetaHeight-1 do
            let color1 = Ix.[x, y]
            let color2 = Iy.[x, y]
            let hypColor = hyp color1 color2
            g.[x, y] <- hypColor;

            //Calc theta
            let theta = (Math.Atan2((float)Iy.[x, y], (float)Ix.[x, y]))
            let num = ((int) (Math.Round(theta * (5.0 / Math.PI))) + 5) % 5
            thetaQ.[x, y] <- num
    
    (g, thetaQ)

let nonMaxSuppresion (image: int[,]) (theta: int[,]) =
    // Non-maximum suppression
    let gradSup: int[,] = image
    let width = image.GetLength(0)
    let height = image.GetLength(1)
    for r in 0 .. width-1 do
        for c in 0 .. height-1 do
            //Suppress pixels at the image edge
            if r = 0 || r = width-1 || c = 0 || c = height - 1 then
                gradSup.[r, c] <- black
            else
                let tq = (int)(theta.[r, c] % 4)
                
                if (tq = 0 && (image.[r, c] <= image.[r, c-1] || image.[r, c] <= image.[r, c+1]))
                    || tq = 1 && (image.[r, c] <= image.[r-1, c+1] || image.[r, c] <= image.[r+1, c-1])
                    || tq = 2 && (image.[r, c] <= image.[r-1, c] || image.[r, c] <= image.[r+1, c])
                    || tq = 3 && (image.[r, c] <= image.[r-1, c-1] || image.[r, c] <= image.[r+1, c+1]) then
                        gradSup.[r, c] <- black
    gradSup

// This gray scale is slow but easy.
// https://web.archive.org/web/20110827032809/http://www.switchonthecode.com/tutorials/csharp-tutorial-convert-a-color-image-to-grayscale
let toGrayScale (image: Bitmap) =
    let width = image.Width
    let height = image.Height
    let imageArr: int[,] = Array2D.zeroCreate width height
    for x in 0 .. width-1 do
        for y in 0 .. height-1 do
            let c = image.GetPixel(x, y);
            let grayScale = (int)(((float)c.R * 0.3) + ((float)c.G * 0.59) + ((float)c.B * 0.11))
            imageArr.[x, y] <- grayScale
    imageArr

let getMax (image: int[,]) = 
    let width = image.GetLength(0)
    let height = image.GetLength(1)
    let mutable max = 0
    for x in 0 .. width-1 do
        for y in 0 .. height-1 do
            if image.[x, y] > max then
                max <- image.[x, y]
    max

let doubleThreashold (image: int[,]) = 
    let highThreshold = (float)(getMax image) * 0.12
    let lowThreshold = highThreshold * 0.07
    let width = image.GetLength(0)
    let height = image.GetLength(1)
    let double: int[,] = Array2D.zeroCreate width height

    for x in 0 .. width-1 do
        for y in 0 .. height-1 do
            if (float)(image.[x, y]) <= lowThreshold then
                double.[x, y] <- black
            else if (float)(image.[x, y]) >= highThreshold then
                double.[x, y] <- white
            else
                double.[x, y] <- weak
    double

let hasStrongNeighbor (image: int[,]) (x: int) (y: int) = 
    let strong = 255
    let width = image.GetLength(0)
    let height = image.GetLength(1)
    
    let mutable result = false
    for i in -1 .. 1 do
        for j in -1 .. 1 do
            let posX = x + i
            let posY = y + j
            // not edges or itself
            if not ((i = 0 && j = 0) || posX <= 0 || posX >= width-1 || posY <= 0 || posY >= height - 1) then
                result <- result || (image.[posX, posY] = strong)
    result

let hysteresis (img: int[,]) = 
    let width = img.GetLength(0)
    let height = img.GetLength(1)
    let image = new Bitmap(width, height)
    let mutable num = 0
    for x in 0 .. width-1 do
        for y in 0 .. height-1 do
            if img.[x, y] = weak then
                if (hasStrongNeighbor img x y) then
                    image.SetPixel(x, y, Color.White)
                    num <- num + 1
                else 
                    image.SetPixel(x, y, Color.Black)
            else
                let value = img.[x, y]
                if (value = white) then
                    num <- num + 1
                image.SetPixel(x, y, Color.FromArgb(value, value, value))
    printfn "Num: %i" num
    image

let arrayToBit (intArr: int[,]) = 
    let width = intArr.GetLength(0)
    let height = intArr.GetLength(1)
    let image = new Bitmap(width, height)
    for x in 0 .. width-1 do
        for y in 0 .. height-1 do
            let value = Math.Min(255, intArr.[x, y])
            image.SetPixel(x, y, Color.FromArgb(value, value, value))
    image

[<EntryPoint>]
let main argv =
    let image: Bitmap = new Bitmap("../download.jpg")
    
    let imageArrGray = toGrayScale image
    
    let gauFilt = GaussianFilter 5 1.0
    let gau = Convolve imageArrGray gauFilt
    
    let (intensity, theta) = computeIntensity gau
    
    let nonMax = nonMaxSuppresion intensity theta
    
    let doubleThreashold = doubleThreashold nonMax
    let hysteresis = hysteresis doubleThreashold
    hysteresis.Save("Final.png")

    0 // return an integer exit code
