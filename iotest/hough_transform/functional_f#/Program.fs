open System
open System.Drawing

let bm = new Bitmap("iotest/hough_transform/Pentagon.png")

[<EntryPoint>]
let main argv =
    let c = bm.Width
    printfn "%i" c
    0 // return an integer exit code
