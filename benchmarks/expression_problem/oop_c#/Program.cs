using System;

namespace oop_c_
{
    class Program
    {
        static Random rand = new Random(2);
        static int number = 0;

        static void Main(string[] args)
        {
            int printCount = 0;
            int evalCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                number = 0;
                IExpression expr = generateRandomExpression(1000);
                printCount += expr.PrettyPrint().Length;
                evalCount += expr.Eval();
            }
            System.Console.WriteLine(printCount);
            System.Console.WriteLine(evalCount);
        }

        static IExpression generateRandomExpression(int max)
        {
            if(number >= max) return new Lit(rand.Next(0, 100));
            else
            {
                number++;
                int exprChoice = rand.Next(0, 4);
                if(exprChoice == 0) return new Add(generateRandomExpression(max), generateRandomExpression(max));
                else if(exprChoice == 1) return new Minus(generateRandomExpression(max), generateRandomExpression(max));
                else if(exprChoice == 2) return new Multiply(generateRandomExpression(max), generateRandomExpression(max));
                else return new Negate(generateRandomExpression(max));
            }
        }
    }

    public interface IExpression
    {
        int Eval();
        string PrettyPrint();
    }

    public class Add : IExpression
    {
        public IExpression left, right;
        public Add(IExpression l, IExpression r) => (left, right) = (l, r);
        public int Eval() => left.Eval() + right.Eval();

        public string PrettyPrint() => "(" + left.PrettyPrint() + "+" + right.PrettyPrint() + ")";
    }

    public class Minus : IExpression
    {
        public IExpression left, right;
        public Minus(IExpression l, IExpression r) => (left, right) = (l, r);
        public int Eval() => left.Eval() - right.Eval();

        public string PrettyPrint() => "(" + left.PrettyPrint() + "-" + right.PrettyPrint() + ")";
    }

    public class Multiply : IExpression
    {
        public IExpression left, right;
        public Multiply(IExpression l, IExpression r) => (left, right) = (l, r);
        public int Eval() => left.Eval() * right.Eval();

        public string PrettyPrint() => "(" + left.PrettyPrint() + "*" + right.PrettyPrint() + ")";
    }

    public class Negate : IExpression
    {
        IExpression child;
        public Negate(IExpression c) { child = c; }
        public int Eval() => -child.Eval();

        public string PrettyPrint() => "(-" + child.PrettyPrint() + ")";
    }

    public class Lit : IExpression
    {
        public int Value;
        public Lit(int v) { Value = v; }
        public int Eval() => Value;

        public string PrettyPrint() => Value.ToString();
    }
}