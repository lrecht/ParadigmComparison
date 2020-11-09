using System;

namespace functional_c_
{

    class Program
    {
        static void Main(string[] args)
        {
            var res = run(1000);
            System.Console.WriteLine(res);
        }

        private static (int, int) run(int runs)
            => runHelper(runs, 0, 0, new Random(2));

        private static (int, int) runHelper(int runs, int printCount, int evalCount, Random rand)
        {
            if(runs <= 0)
                return (printCount, evalCount);

            var (exprPrintFn, exprEvalFn) = generateRandomExpression(1000, rand);
            return runHelper(runs - 1, printCount + exprPrintFn().Length, evalCount + exprEvalFn(), rand);
        }

        private static (Func<string> print, Func<int> eval) generateRandomExpression(int count, Random rand)
        {
            if(count <= 0){
                var lit = rand.Next(0, 100);
                return (() => lit.ToString(), () => lit);
            }

            var x = rand.Next(0, 4);

            switch(x){
                case 0: {
                    var (printFn1, evalFn1) = generateRandomExpression(count - 1, rand);
                    var (printFn2, evalFn2) = generateRandomExpression(0, rand);
                    Func<string> newPrintFn = () => "(" + printFn1() + "+" + printFn2() + ")";
                    Func<int> newEvalFn = () => evalFn1() + evalFn2();
                    return (newPrintFn, newEvalFn);
                };
                case 1: {
                    var (printFn1, evalFn1) = generateRandomExpression(count - 1, rand);
                    var (printFn2, evalFn2) = generateRandomExpression(0, rand);
                    Func<string> newPrintFn = () => "(" + printFn1() + "-" + printFn2() + ")";
                    Func<int> newEvalFn = () => evalFn1() - evalFn2();
                    return (newPrintFn, newEvalFn);
                }
                case 2:{
                    var (printFn1, evalFn1) = generateRandomExpression(count - 1, rand);
                    var (printFn2, evalFn2) = generateRandomExpression(0, rand);
                    Func<string> newPrintFn = () => "(" + printFn1() + "*" + printFn2() + ")";
                    Func<int> newEvalFn = () => evalFn1() * evalFn2();
                    return (newPrintFn, newEvalFn);
                }
                case 3: {
                    var (printFn, evalFn) = generateRandomExpression(count - 1, rand);
                    Func<string> newPrintFn = () => "(-" + printFn() + ")";
                    Func<int> newEvalFn = () => -evalFn();
                    return (newPrintFn, newEvalFn);
                };
                default: throw new Exception("Not reachable");
            }
        }
    }
}
