namespace MathTool;

internal enum TermType
{
    None,
    Number,
    Literal,
    Operator,
    Parenthesis,
    Sign
}

internal record Term
{
    public TermType Type { get; set; }

    public Term? Parent { get; set; }
    public Term? Left { get; set; }
    public Term? Right { get; set; }

    public string? Value { get; set; }

    public Term(TermType type, char value)
    {
        Type = type;
        Value = value.ToString();
    }

    public Term(Term term)
    {
        Type = term.Type;
        Left = term.Left;
        Right = term.Right;
        Value = term.Value;
    }

    public override string ToString()
    {
        var result = "";

        switch (Type)
        {
            case TermType.Number:
            case TermType.Literal:
            case TermType.Parenthesis:
                result += Value;
                break;
            case TermType.Operator:
                result += Value switch
                {
                    "^" => $"{Left?.ToString()}^{Right?.ToString()}",
                    "*" when Left?.Type == TermType.Number && Right?.Left?.Type == TermType.Literal => $"{Left?.ToString()}{Right?.ToString()}",
                    _ => $"{Left?.ToString()} {Value} {Right?.ToString()}",
                };
                break;
            case TermType.Sign:
                result += Value;
                result += Right?.ToString();
                break;
        }

        return result;
    }
}
