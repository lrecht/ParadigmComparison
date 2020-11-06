// Learn more about F# at http://fsharp.org

open System
open System.Text

type IExpr = 
    abstract Print: string
    abstract Eval: int

type ADD =
    {
        Left: IExpr
        Right: IExpr
    }
    member this.Print = 
         "(" + this.Left.Print + "+" + this.Right.Print + ")"
    member this.Eval =
        this.Left.Eval + this.Right.Eval
    interface IExpr with
        member x.Print = x.Print
        override x.Eval = x.Eval

type MIN =
    {
        Left: IExpr
        Right: IExpr
    }
    member this.Print = 
        "(" + this.Left.Print + "-" + this.Right.Print + ")"
    member this.Eval =
        this.Left.Eval - this.Right.Eval
    interface IExpr with
        member x.Print = x.Print
        override x.Eval = x.Eval

type MUL =
    {
        Left: IExpr
        Right: IExpr
    }
    member this.Print = 
        "(" + this.Left.Print + "*" + this.Right.Print + ")"
    member this.Eval =
        this.Left.Eval * this.Right.Eval
    interface IExpr with
        member x.Print = x.Print
        override x.Eval = x.Eval

type VAL = 
    {
        Value: int
    }
    member this.Print = this.Value.ToString()
    member this.Eval = this.Value
    interface IExpr with
        member x.Print = x.Print
        member x.Eval = x.Eval

type NEGATE = 
    {
        Value: IExpr
    }
    member this.Print = "(-" + (this.Value.Print) + ")"
    member this.Eval = -(this.Value.Eval)
    interface IExpr with
        member x.Print = x.Print
        member x.Eval = x.Eval

let rand = Random(2)
let mutable number: int = 0

let rec generateRandomExpression (max: int) = 
    if number >= max then
        let valueType = { VAL.Value = rand.Next(0, 100) }
        (valueType :> IExpr)
    else 
        number <- number + 1
        let test = rand.Next(0, 4)
        if test = 0 then
            let addType = { ADD.Left = (generateRandomExpression max ); ADD.Right = (generateRandomExpression max ) }
            (addType :> IExpr)
        else if test = 1 then
            let minType = { MIN.Left = (generateRandomExpression max ); MIN.Right = (generateRandomExpression max ) }
            (minType :> IExpr)
        else if test = 2 then
            let mulType = { MUL.Left = (generateRandomExpression max ); MUL.Right = (generateRandomExpression max ) }
            (mulType :> IExpr)
        else
            let neg = { NEGATE.Value = (generateRandomExpression max ) }
            (neg :> IExpr)

[<EntryPoint>]
let main argv =
    let stop = System.Diagnostics.Stopwatch.StartNew()
    let stringStop = System.Diagnostics.Stopwatch()
    let evalStop = System.Diagnostics.Stopwatch()
    let mutable stringCount = 0
    let mutable evalCount = 0
    for i in 1 .. 1000 do
        number <- 0
        let test = generateRandomExpression 1000
        stringStop.Start()
        stringCount <- stringCount + test.Print.Length
        stringStop.Stop()
        evalStop.Start()
        evalCount <- evalCount + test.Eval
        evalStop.Stop()
    
    stop.Stop()
    printfn "String count: %i" stringCount
    printfn "Eval count: %i" evalCount
    printfn "Time: %i" stop.ElapsedMilliseconds
    printfn "string: %i" stringStop.ElapsedMilliseconds
    printfn "eval: %i" evalStop.ElapsedMilliseconds
    0 // return an integer exit code
