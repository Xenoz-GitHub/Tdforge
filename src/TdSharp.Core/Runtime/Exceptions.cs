namespace TdSharp.Core;

public class RuntimeException : Exception
{
    public int Line { get; }
    public RuntimeException(string message, int line) : base(message)
    {
        Line = line;
    }
}

public class ReturnException : Exception
{
    public TdValue Value { get; }
    public ReturnException(TdValue value) { Value = value; }
}

public class BreakException : Exception { }
public class ContinueException : Exception { }
