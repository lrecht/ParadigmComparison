// Learn more about F# at http://fsharp.org

open System
open benchmark

[<Interface>]
type IExpression =
    abstract member Eval: int
    abstract member PrettyPrint: string


type Add(left: IExpression, right: IExpression) =
    interface IExpression with
        member __.Eval = left.Eval + right.Eval
        member __.PrettyPrint = "(" + left.PrettyPrint + "+" + right.PrettyPrint + ")"


type Minus(left: IExpression, right: IExpression) =
    interface IExpression with
        member __.Eval = left.Eval - right.Eval
        member __.PrettyPrint = "(" + left.PrettyPrint + "-" + right.PrettyPrint + ")"


type Multiply(left: IExpression, right: IExpression) =
    interface IExpression with
        member __.Eval = left.Eval * right.Eval
        member __.PrettyPrint = "(" + left.PrettyPrint + "*" + right.PrettyPrint + ")"


type Negate(child: IExpression) =
    interface IExpression with
        member __.Eval = -child.Eval
        member __.PrettyPrint = "(-" + child.PrettyPrint + ")"


type Lit(value) =
    interface IExpression with
        member __.Eval = value
        member __.PrettyPrint = value.ToString()


let mutable random = Random(2)
let mutable number = 0

let rec generateRandomExpression max : IExpression =
    if number >= max 
    then Lit(random.Next(0, 100)) :> IExpression
    else
        number <- number + 1
        let exprChoice = random.Next(0, 4)
        match exprChoice with
        | 0 -> Add(generateRandomExpression(max), generateRandomExpression(max)) :> IExpression
        | 1 -> Minus(generateRandomExpression(max), generateRandomExpression(max)) :> IExpression
        | 2 -> Multiply(generateRandomExpression(max), generateRandomExpression(max)) :> IExpression
        | _ -> Negate(generateRandomExpression(max)) :> IExpression

[<EntryPoint>]
let main argv =
    let iterations = if argv.Length > 0 then int (argv.[0]) else 1
    let bm = Benchmark(iterations)

    bm.Run((fun () ->
        random <- Random(2)
        let mutable printCount = 0
        let mutable evalCount = 0
        for i in 0 .. 999 do
            number <- 0
            let expr = generateRandomExpression(1000)
            printCount <- printCount + expr.PrettyPrint.Length
            evalCount <- evalCount + expr.Eval
        (printCount, evalCount)
    ), (fun (printCount, evalCount) ->
        printfn "%d" printCount
        printfn "%d" evalCount
    ))
    0 // return an integer exit code
