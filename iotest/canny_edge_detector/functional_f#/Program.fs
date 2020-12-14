open System
open System.Drawing

[<EntryPoint>]
let main argv =
    let image = new Bitmap("iotest/canny_edge_detector/download.jpg")
    printfn "%i" image.Width
    0 // return an integer exit code
