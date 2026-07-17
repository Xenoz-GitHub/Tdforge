namespace TdSharp.Core;

public class TdFunction
{
    public string Name { get; set; }
    public List<string> Parameters { get; set; }
    public List<Stmt> Body { get; set; }
    public Environment Closure { get; set; }
    public bool IsConstructor { get; set; }
    public string ClassName { get; set; }

    public TdFunction(string name, List<string> parameters, List<Stmt> body, Environment closure, bool isConstructor = false, string className = null)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
        Closure = closure;
        IsConstructor = isConstructor;
        ClassName = className;
    }

    public int Arity => Parameters?.Count ?? 0;
}
