using System.Reflection.Metadata.Ecma335;

namespace MathTool;

internal class Function
{
    private Term _root;

    public Function(string function)
    {
        Parse(function);

        ArgumentNullException.ThrowIfNull(_root, nameof(function));
    }

    // TODO: String for now
    public string GetDerivative() => "";

    public override string ToString()
    {
        return _root.ToString();
    }

    private static TermType GetTermType(char c) => c switch
    {
        >= '0' and <= '9' or ',' => TermType.Number,
        '+' or '-' or '*' or '/' or '^' => TermType.Operator,
        >= 'A' and <= 'Z' or >= 'a' and <= 'z' => TermType.Literal,
        '(' or ')' => TermType.Parenthesis,
        _ => TermType.None,
    };

    private void Parse(string input)
    {
        ArgumentNullException.ThrowIfNull(input, nameof(input));

        input = input.Replace(" ", "");

        List<Term> terms = [];
        Stack<Term> stack = [];

        Term? previous = null;
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            var term = GetTermType(c) switch
            {
                TermType.Number => new Term(TermType.Number, c),
                TermType.Operator => new Term(TermType.Operator, c),
                TermType.Literal => new Term(TermType.Literal, c),
                TermType.Parenthesis => new Term(TermType.Parenthesis, c),
                _ => throw new ArgumentException($"Invalid character: {c} at Position: {i}"),
            };

            if (term.Type == TermType.Parenthesis)
            {
                if (c == '(')
                {
                    if (previous != null && previous.Type != TermType.Operator && previous.Type != TermType.Parenthesis)
                        PerformMultiplication(previous, term);

                    stack.Push(term);
                }
                else if (c == ')')
                {
                    List<Term> subTerms = [];
                    while (stack.Peek().Value != "(")
                        subTerms.Insert(0, stack.Pop());
                    stack.Pop();

                    var subExpression = BuildExpression(subTerms);
                    stack.Push(subExpression);
                }

                continue;
            }

            // Term will never be null here
            if (stack.Count > 0)
                stack.Push(term);
            else
                terms.Add(term);

            if (previous == null)
            {
                previous = term;
                continue;
            }

            void PerformMultiplication(Term first, Term second)
            {
                var multiplyTerm = new Term(TermType.Operator, '*');
                var index = terms.IndexOf(first);
                if (index == -1) index = terms.Count - 1;
                terms.Insert(index + 1, multiplyTerm);

                if (second.Parent != null)
                    second.Parent.Right = multiplyTerm;

                SetLeft(multiplyTerm, second);
                SetRight(multiplyTerm, first);
            }

            switch (previous.Type)
            {
                case TermType.Number:
                case TermType.Literal:
                    var isLiteral = previous.Type == TermType.Literal;
                    switch (term.Type)
                    {
                        case TermType.Number: // 1|x 2
                            if (isLiteral)
                                throw new ArgumentException($"Invalid term: {previous.Value} {term.Value} at Position: {i}");
                            
                            previous.Value += c;
                            if (stack.Count == 0)
                                terms.Remove(term);
                            continue;
                        case TermType.Literal: // 1|x x
                            if (previous.Parent?.Type == TermType.Sign)
                                SetRight(previous.Parent, term);
                            PerformMultiplication(term, previous);
                            break;
                        case TermType.Operator: // 1|x -
                            term.Left = previous.Parent?.Type == TermType.Sign ? previous.Parent : previous;
                            break;
                            // TODO: Implement Parenthesis
                    }
                    break;
                case TermType.Operator:
                    switch (term.Type)
                    {
                        case TermType.Number: // + 1
                        case TermType.Literal: // + x
                            if (previous.Left == null)
                                previous.Type = TermType.Sign;
                            SetRight(previous, term);
                            break;
                        case TermType.Operator: // + +
                            // We simplify the expression by removing the sign and updating the previous operator if it is either + or -
                            if (previous.Value == "+" && term.Value == "-")
                                previous.Value = "-";
                            else if (previous.Value == "-" && term.Value == "-")
                                previous.Value = "+";

                            terms.Remove(term);
                            continue;
                        // TODO: Implement Parenthesis
                    }
                    break;
                case TermType.Parenthesis:
                    // TODO: Implement Parenthesis
                    break;
                case TermType.Sign:
                    switch (term.Type)
                    {
                        case TermType.Number: // - 1
                        case TermType.Literal: // - x
                            SetRight(previous, term);
                            break;
                    };
                    break;
            }

            if (previous.Parent != null && previous.Parent.Type == TermType.Operator)
                if (previous.Parent.Right == term.Left)
                {
                    SetRight(previous.Parent, term);
                    term.Left!.Parent = term;
                }

            previous = term;
        }

        if (stack.Count > 0)
            terms.AddRange(stack);

        _root = BuildExpression(terms);
    }

    private Term BuildExpression(List<Term> terms)
    {
        foreach (var term in terms)
            if (term.Type == TermType.Operator && term.Left != null && term.Right != null)
                return term;

        throw new InvalidOperationException("Invalid expression");
    }

    private static void SetRight(Term parent, Term term)
    {
        parent.Right = term;
        term.Parent = parent;
    }

    private static void SetLeft(Term parent, Term term)
    {
        parent.Left = term;
        term.Parent = parent;
    }
}
