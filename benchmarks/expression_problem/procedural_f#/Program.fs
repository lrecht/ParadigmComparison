// Learn more about F# at http://fsharp.org

open System

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

[<EntryPoint>]
let main argv =
    let test = 
        { 
            ADD.Left = { VAL.Value = 3};
            ADD.Right = 
            { 
                MUL.Left = 
                    { 
                        MIN.Left = 
                            { 
                                MIN.Left = { VAL.Value = 3};
                                MIN.Right = { VAL.Value = 3}
                            };
                        MIN.Right = { VAL.Value = 3}
                    };
                MUL.Right = 
                    {
                        MUL.Left = 
                            { 
                                MIN.Left = 
                                    { 
                                        MIN.Left = { VAL.Value = 3};
                                        MIN.Right = { VAL.Value = 3}
                                    };
                                MIN.Right = { VAL.Value = 3}
                            };
                        MUL.Right = { VAL.Value = 3}
                    }
            }
        }
    printfn "%s = %i" test.Print test.Eval
    
    0 // return an integer exit code
