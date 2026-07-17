namespace TdSharp.Core;

public class TdObject
{
    public string ClassName { get; set; }
    public Dictionary<string, TdValue> Fields { get; set; } = new();
    public Dictionary<string, TdFunction> Methods { get; set; } = new();

    public TdObject(string className)
    {
        ClassName = className;
    }

    public override string ToString()
    {
        return $"{ClassName}({string.Join(", ", Fields.Select(kv => kv.Key + "=" + kv.Value))})";
    }
}
