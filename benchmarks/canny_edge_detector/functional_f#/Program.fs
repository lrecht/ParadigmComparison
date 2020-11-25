open System
open System.Drawing

let weak = 100
let kernelHor = Array.mapi (fun index v -> (index%3-1,index/3-1,v)) [|-1.0; 0.0; 1.0; -2.0; 0.0; 2.0; -1.0; 0.0; 1.0|]
let kernelVer = Array.mapi (fun index v -> (index%3-1,index/3-1,v)) [|1.0; 2.0; 1.0; 0.0; 0.0; 0.0; -1.0; -2.0; -1.0|]

let fst (c,_,_) = c
let snd (_,c,_) = c
let trd (_,_,c) = c

let getWH (a:(int*int*int)array) =
    fst a.[a.Length-1] + 1,
    snd a.[a.Length-1] + 1

// The following 13 lines are from http://www.fssnip.net/sn/title/Bitmap-Primitives-Helpers
let toRgbArray (bmp : Bitmap) =
    [| for y in 0..bmp.Height-1 do
       for x in 0..bmp.Width-1 -> x,y,(bmp.GetPixel(x,y)) |]   

let toBitmap (a:(int*int*int) array) =
    let width,height = getWH a
    let bmp = new Bitmap(width, height)
    a |> Array.Parallel.iter (fun (x,y,c) -> bmp.SetPixel(x,y,Color.FromArgb(c,c,c)))
    bmp

let greyOnePixel (x,y,c :Color) =
    let gscale = int((float c.R * 0.3) + (float c.G * 0.59) + (float c.B * 0.11))
    x,y,gscale

let greyScale image =
    Array.map greyOnePixel image

let createGaussianFilter length weight =
    let foff = (length - 1) / 2
    let constant = 1.0 / (2.0 * Math.PI * weight * weight)
    let kernel = Array.create (length * length) 0.0
    let filter = Array.mapi (fun x _ -> 
                        let offX = (x % length) - foff
                        let offY = (x / length) - foff
                        (offX,offY,constant * Math.Exp(-(float(offY*offY + (offX * offX)) / (2.0 * weight * weight))))) kernel
    let sum = Array.fold (fun sum (_,_,x) -> sum+x) 0.0 filter
    Array.map (fun (a,b,x) -> a,b,x/sum) filter

let convolveOne (image:(int*int*int)array) imageLength imageHeight filter (x,y,_) =
    x,y,(int)(Array.fold (fun sum (fx,fy,weight) -> 
                    let xIndex = (x+fx)
                    let yIndex = (y+fy)
                    if xIndex < 0 || yIndex < 0 || xIndex > imageLength-1 || yIndex > imageHeight-1
                    then sum
                    else sum + weight * float (trd (image.[xIndex+yIndex*imageLength]))) 0.0 filter)

let convolve (image:(int*int*int)array) filter = 
    let imageLength,imageHeight = getWH image
    Array.map ((fun values -> convolveOne image imageLength imageHeight filter values)) image

let gFilter length weight image =
    let filter = createGaussianFilter length weight
    convolve image filter

let hyp x y =
    (int)(Math.Sqrt((float)(x * x) + (float)(y * y)))

let arctan x y =
    ((int) (Math.Round((Math.Atan2((float)x, (float)y)) * (5.0 / Math.PI))) + 5) % 5

let intensityGradients image = 
    let hor = convolve image kernelHor
    let ver = convolve image kernelVer
    let gradient = Array.map2 (fun (x1,y1,w1) (x2,y2,w2) -> (x1,y1,hyp w1 w2)) hor ver
    let direction = Array.map2 (fun (x1,y1,w1) (x2,y2,w2) -> (x1,y1,arctan w1 w2)) hor ver
    (gradient,direction)

let maxSuppressionOne width height (image:(int*int*int)array) x y w1 w2 =
    if x = 0 || x = width-1 || y = 0 || y = height - 1 then
        w1
    else
        let tq = (int)(w2 % 4)
        if (tq = 0 && (w1 <= trd image.[x+(y-1)*width] || w1 <= trd image.[x+(y+1)*width])) //0 is E-W (horizontal)
         ||(tq = 1 && (w1 <= trd image.[x-1+(y+1)*width] || w1 <= trd image.[x+1+(y-1)*width])) //1 is NE-SW
         ||(tq = 2 && (w1 <= trd image.[x-1+y*width] || w1 <= trd image.[x+1+y*width])) //2 is N-S (vertical)
         ||(tq = 3 && (w1 <= trd image.[x-1+(y-1)*width] || w1 <= trd image.[x+1+(y+1)*width])) //#3 is NW-SE
        then 0
        else w1

let nonMaxSuppression ((gradient:(int*int*int) array),direction) = 
    let width,height = getWH gradient
    Array.map2 (fun (x,y,w1) (_,_,w2) -> x,y,maxSuppressionOne width height gradient x y w1 w2) gradient direction

let doubleThreshold image = 
    let max = Array.fold (fun acc (_,_,w) -> if w > acc then w else acc) 0 image
    let high = (float max) * 0.12
    let low = high*0.07
    Array.map (fun (x,y,w) -> x,y,if float w <= low then 0 elif float w < high then weak else 255) image

let strongNeighbour (image:(int*int*int)array) width height x y =
    [|for i in -1 .. 1 do for j in -1 .. 1 do (i,j)|]
    |> Array.fold (fun acc (fx,fy) ->
                   let posX = x+fx
                   let posY = y+fy
                   if not ((fx = 1 && fy = 1) || posX <= 0 || posX >= width-1 || posY <= 0 || posY >= height - 1)
                   then acc || (trd image.[posX+posY*width] = 255)
                   else acc) false

let hysteresis image =
    let width,height = getWH image
    Array.map (fun (x,y,w) -> 
                    x,y,if w = weak then
                            if strongNeighbour image width height x y
                            then 255
                            else 0
                        else w) image

let cannyBoi image =
    greyScale image
    |> gFilter 5 1.0
    |> intensityGradients
    |> nonMaxSuppression
    |> doubleThreshold
    |> hysteresis

[<EntryPoint>]
let main argv =
    let image = new Bitmap("benchmarks/canny_edge_detector/download.jpg")
    let res = cannyBoi (toRgbArray image)
    //(res |> toBitmap).Save("Final.png")
    printfn "%i" (Array.fold (fun acc (_,_,w) -> if w > 0 then acc + 1 else acc) 0 res)
    0 // return an integer exit code
