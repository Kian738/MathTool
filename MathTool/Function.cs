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

            // Term will never be null here
            terms.Add(term);

            if (previous == null)
            {
                previous = term;
                continue;
            }

            void PerformMultiplication(Term term, Term previous)
            {
                var multiplyTerm = new Term(TermType.Operator, '*');
                var index = terms.IndexOf(term) - 1;
                terms.Insert(index, multiplyTerm);

                if (previous.Parent != null)
                    previous.Parent.Right = multiplyTerm;

                SetLeft(multiplyTerm, previous);
                SetRight(multiplyTerm, term);
            }

            switch (previous.Type)
            {
                case TermType.Number:
                    switch (term.Type)
                    {
                        case TermType.Number: // 1 2
                            previous.Value += c;
                            terms.Remove(term);
                            continue;
                        case TermType.Literal: // 1 x
                            PerformMultiplication(term, previous);
                            break;
                        case TermType.Operator: // 1 -
                            term.Left = previous;
                            break;
                            // TODO: Implement Parenthesis
                    }
                    break;
                case TermType.Literal:
                    switch (term.Type)
                    {
                        case TermType.Number: // x 1
                            throw new ArgumentException($"Invalid term: {previous.Value} {term.Value} at Position: {i}"); // TODO: Beautify the error
                        case TermType.Literal: // x x
                            PerformMultiplication(previous, term);
                            break;
                        case TermType.Operator: // x -
                            term.Left = previous;
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
                            else
                                SetRight(previous, term);
                            break;
                        case TermType.Operator: // + +

                            break;
                        // TODO: Implement Parenthesis
                    }
                    break;
                case TermType.Parenthesis:
                    // TODO: Implement Parenthesis
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

        foreach (var term in terms)
            if (term.Type == TermType.Operator && term.Left != null && term.Right != null)
            {
                _root = term;
                break;
            }

        void SetRight(Term parent, Term term)
        {
            parent.Right = term;
            term.Parent = parent;
        }

        void SetLeft(Term parent, Term term)
        {
            parent.Left = term;
            term.Parent = parent;
        }
    }
}
