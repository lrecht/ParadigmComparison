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
let rec generateRandomExpression count = 
    if count <= 0 then Var (rand.Next(0, 100))
    else 
        match rand.Next(0,4) with
          | 0 -> Add (generateRandomExpression (count-1),generateRandomExpression 0)
          | 1 -> Min (generateRandomExpression (count-1),generateRandomExpression 0)
          | 2 -> Mul (generateRandomExpression (count-1),generateRandomExpression 0)
          | 3 -> Neg (generateRandomExpression (count-1))

let rec run' digit printCount evalCount =
    match digit with
      | 0 -> (printCount,evalCount)
      | _ -> let expr = (generateRandomExpression 1000)
             run' (digit-1) (printCount+(Seq.length (prettyPrint expr))) (evalCount+(eval expr))

let run digit =
    run' digit 0 0

[<EntryPoint>]
let main argv =
    printfn "%O" (run 1000)
    0 // return an integer exit code
