namespace TdSharp.Core;

public class Environment
{
    internal readonly Dictionary<string, TdValue> _values = new();
    public Environment Enclosing { get; }
    public IReadOnlyDictionary<string, TdValue> AllValues => _values;

    public Environment() { Enclosing = null; }

    public Environment(Environment enclosing)
    {
        Enclosing = enclosing;
    }

    public void Define(string name, TdValue value)
    {
        _values[name] = value;
    }

    public TdValue Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value))
            return value;
        if (Enclosing != null)
            return Enclosing.Get(name);
        throw new RuntimeException($"Undefined variable '{name.Lexeme}'.", name.Line);
    }

    public TdValue GetAt(int distance, string name)
    {
        var env = Ancestor(distance);
        if (env != null && env._values.TryGetValue(name, out var val))
            return val;
        throw new RuntimeException($"Undefined variable '{name}'.", 0);
    }

    public void Assign(Token name, TdValue value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }
        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }
        // Auto-create global variable on assignment (Td# convention)
        // This allows x = 5 without declaring var x first, but masks typos
        // To disable auto-create, uncomment the throw below and comment the line after:
        // throw new RuntimeException($"Undefined variable '{name.Lexeme}'.", name.Line);
        _values[name.Lexeme] = value;
    }

    public void AssignAt(int distance, Token name, TdValue value)
    {
        var env = Ancestor(distance);
        if (env != null)
            env._values[name.Lexeme] = value;
    }

    public bool Has(Token name)
    {
        if (_values.ContainsKey(name.Lexeme)) return true;
        if (Enclosing != null) return Enclosing.Has(name);
        return false;
    }

    private Environment Ancestor(int distance)
    {
        Environment env = this;
        for (int i = 0; i < distance; i++)
        {
            if (env == null) break;
            env = env.Enclosing;
        }
        return env;
    }
}

