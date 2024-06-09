namespace MathTool;

class Program
{
    static void Main()
    {
        // TODO: Use a testing framework
        List<string> testFunctionStrings = ["-x^2 + 5x - 1", "3x^2 - (1+3)x" /* TODO: Fix */, "3x^2 - (1/3)x", "24,62x + 5", "2x^4 - 6x^3 + x^2 + x^0,5 - 4", "-4x^2 - +2x + -2" /* TODO: Fix */]; // TODO: Support f(x) =
        foreach (var functionString in testFunctionStrings)
        {
            var function = new Function(functionString);
            Console.WriteLine($"Original: {function.ToString()}");
            Console.WriteLine($"Derivative: {function.GetDerivative()}");
        }
    }
}