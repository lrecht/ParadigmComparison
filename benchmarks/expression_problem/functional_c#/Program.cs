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

            var max = 1000;
            var count = 0;
            var (exprPrintFn, exprEvalFn) = generateRandomExpression(max, ref count, rand);
            return runHelper(runs - 1, printCount + exprPrintFn().Length, evalCount + exprEvalFn(), rand);
        }

        private static (Func<string> print, Func<int> eval) generateRandomExpression(int max, ref int count, Random rand)
        {
            if(count >= max){
                var lit = rand.Next(0, 100);
                return (() => lit.ToString(), () => lit);
            }

            count++;
            var x = rand.Next(0, 4);

            switch(x){
                case 0: {
                    var (printFn1, evalFn1) = generateRandomExpression(max, ref count, rand);
                    var (printFn2, evalFn2) = generateRandomExpression(max, ref count, rand);
                    Func<string> newPrintFn = () => "(" + printFn1() + "+" + printFn2() + ")";
                    Func<int> newEvalFn = () => evalFn1() + evalFn2();
                    return (newPrintFn, newEvalFn);
                };
                case 1: {
                    var (printFn1, evalFn1) = generateRandomExpression(max, ref count, rand);
                    var (printFn2, evalFn2) = generateRandomExpression(max, ref count, rand);
                    Func<string> newPrintFn = () => "(" + printFn1() + "-" + printFn2() + ")";
                    Func<int> newEvalFn = () => evalFn1() - evalFn2();
                    return (newPrintFn, newEvalFn);
                }
                case 2:{
                    var (printFn1, evalFn1) = generateRandomExpression(max, ref count, rand);
                    var (printFn2, evalFn2) = generateRandomExpression(max, ref count, rand);
                    Func<string> newPrintFn = () => "(" + printFn1() + "*" + printFn2() + ")";
                    Func<int> newEvalFn = () => evalFn1() * evalFn2();
                    return (newPrintFn, newEvalFn);
                }
                case 3: {
                    var (printFn, evalFn) = generateRandomExpression(max, ref count, rand);
                    Func<string> newPrintFn = () => "(-" + printFn() + ")";
                    Func<int> newEvalFn = () => -evalFn();
                    return (newPrintFn, newEvalFn);
                };
                default: throw new Exception("Not reachable");
            }
        }
    }
}
