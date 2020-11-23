// Learn more about F# at http://fsharp.org

open System
open System.Drawing

let weak = 100
let black = Color.FromArgb(0, 0, 0)
let white = Color.FromArgb(255, 255, 255)
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
let Convolve (image: Bitmap) (kernel: float[,]) =
    let width = image.Width
    let height = image.Height
    let test = new Bitmap(width, height)
    //Kernel has to be an odd number
    let halfKernel = kernel.GetLength(0) / 2
    for x in 0 .. width-1 do
        for y in 0 .. height-1 do
            let mutable sum = 0.0
            for kx in -halfKernel .. halfKernel do
                for ky in -halfKernel .. halfKernel do
                    let posX = x + kx
                    let posY = y + ky
                    // not edges
                    if not (posX <= 0 || posX >= width-1 || posY <= 0 || posY >= height - 1) then
                        sum <- sum + ((float)(getPixel image posX posY) * (kernel.[kx+halfKernel, ky+halfKernel]))
            
            if (sum > 255.0) then
                sum <- 255.0
            else if (sum < 0.0) then
                sum <- 0.0

            test.SetPixel(x, y, Color.FromArgb((int)sum, (int)sum, (int)sum))
    test

let hyp (num1: int) (num2: int) = 
    let hyp = (Math.Sqrt((float)(num1 * num1) + (float)(num2 * num2)))
    Math.Min(255, (int)hyp)

let hypot (image1: Bitmap) (image2: Bitmap) = 
    let result: Bitmap = new Bitmap(image1.Width, image1.Height)
    
    for x in 0 .. image1.Width-1 do
        for y in 0 .. image1.Height-1 do
            let color1 = getPixel image1 x y
            let color2 = getPixel image2 x y
            
            let hypColor = hyp color1 color2
            let newColor = Color.FromArgb(hypColor, hypColor, hypColor);
            result.SetPixel(x, y, newColor)
    result
    
let arctan (image1: Bitmap) (image2: Bitmap) = 
    let result: (double)[,] = Array2D.create image1.Width image1.Height (0.0)
    
    for x in 0 .. image1.Width-1 do
        for y in 0 .. image1.Height-1 do
            let color1 = image1.GetPixel(x, y)
            let color2 = image2.GetPixel(x, y)
            result.[x, y] <- (Math.Atan2((float)color1.R, (float)color2.R))
    result


let computeIntensity (image: Bitmap) = 
    let Ix = Convolve image kernelHor
    let Iy = Convolve image kernelVer
    
    let g = hypot Ix Iy
    
    let theta = arctan Iy Ix
    let thetaQ = Array2D.create (theta.GetLength(0)) (theta.GetLength(1)) 0
    for i in 0 .. theta.GetLength(0)-1 do
        for j in 0 .. theta.GetLength(1)-1 do
            let num = ((int) (Math.Round(theta.[i,j] * (5.0 / Math.PI))) + 5) % 5
            thetaQ.[i, j] <- num
    
    (g, thetaQ)

let nonMaxSuppresion (image: Bitmap) (theta: int[,]) =
    // Non-maximum suppression
    let gradSup: Bitmap = image

    for r in 0 .. image.Width-1 do
        for c in 0 .. image.Height-1 do
            //Suppress pixels at the image edge
            if r = 0 || r = image.Width-1 || c = 0 || c = image.Height - 1 then
                gradSup.SetPixel(r, c, black)
            
            else
                let tq = (int)(theta.[r, c] % 4)
                if tq = 0 then //0 is E-W (horizontal)
                    if image.GetPixel(r, c).R <= image.GetPixel(r, c-1).R || image.GetPixel(r, c).R <= image.GetPixel(r, c+1).R then
                        gradSup.SetPixel(r, c, black)
                if tq = 1 then //1 is NE-SW
                    if image.GetPixel(r, c).R <= image.GetPixel(r-1, c+1).R || image.GetPixel(r, c).R <= image.GetPixel(r+1, c-1).R then
                        gradSup.SetPixel(r, c, black)
                if tq = 2 then //2 is N-S (vertical)
                    if image.GetPixel(r, c).R <= image.GetPixel(r-1, c).R || image.GetPixel(r, c).R <= image.GetPixel(r+1, c).R then
                        gradSup.SetPixel(r, c, black)
                if tq = 3 then //#3 is NW-SE
                    if image.GetPixel(r, c).R <= image.GetPixel(r-1, c-1).R || image.GetPixel(r, c).R <= image.GetPixel(r+1, c+1).R then
                        gradSup.SetPixel(r, c, black)
    gradSup

// This gray scale is slow but easy.
// https://web.archive.org/web/20110827032809/http://www.switchonthecode.com/tutorials/csharp-tutorial-convert-a-color-image-to-grayscale
let toGrayScale (image: Bitmap) =
    for x in 0 .. image.Width-1 do
        for y in 0 .. image.Height-1 do
            let c = image.GetPixel(x, y);
            let grayScale = (int)(((float)c.R * 0.3) + ((float)c.G * 0.59) + ((float)c.B * + 0.11))
            image.SetPixel(x, y, Color.FromArgb(grayScale, grayScale, grayScale));
    image

let maxValue (image: Bitmap) = 
    let mutable max = 0
    for x in 0 .. image.Width-1 do
        for y in 0 .. image.Height-1 do
            if (int)(image.GetPixel(x, y).R) > max then
                max <- (int)(image.GetPixel(x, y).R)
    max

let doubleThreashold (image: Bitmap) = 
    //let highThreshold = 50.0
    let highThreshold = (float)(maxValue image) * 0.09
    //let lowThreshold = 10.0
    let lowThreshold = highThreshold * 0.05
    let double = new Bitmap(image.Width, image.Height)

    for x in 0 .. image.Width-1 do
        for y in 0 .. image.Height-1 do
            if (float)(image.GetPixel(x, y).R) <= lowThreshold then
                double.SetPixel(x, y, black)
            else if (float)(image.GetPixel(x, y).R) >= highThreshold then
                double.SetPixel(x, y, white)
            else
                double.SetPixel(x, y, Color.FromArgb(weak, weak, weak))
    double

let hasStrongNeighbor (image: Bitmap) (x: int) (y: int) = 
    let strong = 0
    
    let mutable result = false
    for i in -1 .. 1 do
        for j in -1 .. 1 do
            let posX = x + i
            let posY = y + j
            // not edges or itself
            if not ((i = 1 && j = 1) || posX <= 0 || posX >= image.Width-1 || posY <= 0 || posY >= image.Height - 1) then
                result <- result || ((getPixel image posX posY) = strong)
    result

let hysteresis (img: Bitmap) = 
    let image = new Bitmap(img.Width, img.Height)
    for x in 0 .. img.Width-1 do
        for y in 0 .. img.Height-1 do
            if (int)(img.GetPixel(x, y).R) = weak then
                if (hasStrongNeighbor img x y) then
                    image.SetPixel(x, y, black)
                else 
                    image.SetPixel(x, y, white)
            else
                image.SetPixel(x, y, img.GetPixel(x, y))
    image

[<EntryPoint>]
let main argv =
    let stop = System.Diagnostics.Stopwatch.StartNew()
    //let image: Bitmap = new Bitmap("benchmarks/canny_edge_detector/download.jpg")
    let mutable image: Bitmap = new Bitmap("../download.jpg")
    
    image <- toGrayScale image
    
    let gauFilt = GaussianFilter 5 1.0
    let gau = Convolve image gauFilt
    
    let (intensity, theta) = computeIntensity gau
    
    let nonMax = nonMaxSuppresion intensity theta
    
    let doubleThreashold = doubleThreashold nonMax
    
    let hysteresis = hysteresis doubleThreashold
    hysteresis.Save("Final.png")
    
    stop.Stop()
    printfn "Time: %i" stop.ElapsedMilliseconds
    0 // return an integer exit code

// Steps:
// 1. Noise reduction. May be performed by Gaussian filter
// 2. Compute intensity gradient
// 3. Non-maximum suppression.
// 4. Tracing edges with hysteresis.

