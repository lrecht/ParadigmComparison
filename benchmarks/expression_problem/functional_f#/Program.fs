type Expr = Add of Expr * Expr
          | Var of int
          | Neg of Expr
          | Mul of Expr * Expr
          | Min of Expr * Expr

let rec prettyPrint e =
    match e with
      | Add(expr1,expr2) -> sprintf "(%s+%s)" (prettyPrint expr1) (prettyPrint expr2)
      | Var(num) -> sprintf "%i" num
      | Neg(expr) -> sprintf "(-%s)" (prettyPrint expr)
      | Mul(expr1,expr2) -> sprintf "(%s*%s)" (prettyPrint expr1) (prettyPrint expr2)
      | Min(expr1,expr2) -> sprintf "(%s-%s)" (prettyPrint expr1) (prettyPrint expr2)

let rec eval e =
    match e with
      | Add(expr1,expr2) -> (eval expr1) + (eval expr2)
      | Var(num) -> num
      | Neg(expr) -> -(eval expr)
      | Mul(expr1,expr2) -> (eval expr1) * (eval expr2)
      | Min(expr1,expr2) -> (eval expr1) - (eval expr2)

let rand = System.Random(2)
let mutable number: int = 0
let rec generateRandomExpression (max: int) = 
    if number >= max then Var (rand.Next(0, 100))
    else 
        number <- number + 1
        match rand.Next(0,4) with
          | 0 -> Add ((generateRandomExpression max),(generateRandomExpression max))
          | 1 -> Min ((generateRandomExpression max),(generateRandomExpression max))
          | 2 -> Mul ((generateRandomExpression max),(generateRandomExpression max))
          | 3 -> Neg (generateRandomExpression max)

let rec run' digit printCount evalCount =
    match digit with
      | 0 -> (printCount,evalCount)
      | _ -> let expr = (generateRandomExpression 1000)
             number <- 0
             run' (digit-1) (printCount+(Seq.length (prettyPrint expr))) (evalCount+(eval expr))

let run digit =
    run' digit 0 0

[<EntryPoint>]
let main argv =
    printfn "%O" (run 1000)
    0 // return an integer exit code
