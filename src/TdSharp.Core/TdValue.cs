namespace TdSharp.Core;

public enum TdValueType
{
    Nil, Bool, Number, String, Array, Map,
    Object, Function, NativeFunction,
    Vector2, Rect, Color, Sprite
}

public class TdValue
{
    public TdValueType Type { get; private set; }
    public bool BoolValue { get; private set; }
    public double NumValue { get; private set; }
    public string StrValue { get; private set; }
    public List<TdValue> ArrayValue { get; private set; }
    public Dictionary<string, TdValue> MapValue { get; private set; }
    public TdObject ObjectValue { get; private set; }
    public TdFunction FunctionValue { get; private set; }
    public Func<Interpreter, List<TdValue>, TdValue> NativeFunc { get; private set; }

    public double VecX { get; private set; }
    public double VecY { get; private set; }

    public double RectX { get; private set; }
    public double RectY { get; private set; }
    public double RectW { get; private set; }
    public double RectH { get; private set; }

    public byte R { get; private set; }
    public byte G { get; private set; }
    public byte B { get; private set; }
    public byte A { get; private set; }

    public string SpritePath { get; private set; }

    private TdValue() { }

    public static TdValue NilVal() => new() { Type = TdValueType.Nil };
    public static TdValue BoolVal(bool v) => new() { Type = TdValueType.Bool, BoolValue = v };
    public static TdValue NumVal(double v) => new() { Type = TdValueType.Number, NumValue = v };
    public static TdValue StrVal(string v) => new() { Type = TdValueType.String, StrValue = v };
    public static TdValue ArrVal(List<TdValue> v) => new() { Type = TdValueType.Array, ArrayValue = v };
    public static TdValue MapVal(Dictionary<string, TdValue> v) => new() { Type = TdValueType.Map, MapValue = v };
    public static TdValue ObjVal(TdObject v) => new() { Type = TdValueType.Object, ObjectValue = v };
    public static TdValue FuncVal(TdFunction v) => new() { Type = TdValueType.Function, FunctionValue = v };
    public static TdValue NativeFuncVal(Func<Interpreter, List<TdValue>, TdValue> v) => new() { Type = TdValueType.NativeFunction, NativeFunc = v };

    public static TdValue Vec2Val(double x, double y) => new()
    {
        Type = TdValueType.Vector2, VecX = x, VecY = y
    };

    public static TdValue RectVal(double x, double y, double w, double h) => new()
    {
        Type = TdValueType.Rect, RectX = x, RectY = y, RectW = w, RectH = h
    };

    public static TdValue ColorVal(byte r, byte g, byte b, byte a) => new()
    {
        Type = TdValueType.Color, R = r, G = g, B = b, A = a
    };

    public static TdValue SpriteVal(string path) => new()
    {
        Type = TdValueType.Sprite, SpritePath = path
    };

    public bool IsTruthy()
    {
        return Type switch
        {
            TdValueType.Nil => false,
            TdValueType.Bool => BoolValue,
            TdValueType.Number => NumValue != 0,
            TdValueType.String => StrValue.Length > 0,
            _ => true
        };
    }

    public override string ToString()
    {
        return Type switch
        {
            TdValueType.Nil => "nil",
            TdValueType.Bool => BoolValue ? "true" : "false",
            TdValueType.Number => NumValue == Math.Floor(NumValue) ? NumValue.ToString("F0") : NumValue.ToString("G"),
            TdValueType.String => StrValue,
            TdValueType.Array => "[" + string.Join(", ", ArrayValue) + "]",
            TdValueType.Map => "{" + string.Join(", ", MapValue.Select(kv => kv.Key + "=" + kv.Value)) + "}",
            TdValueType.Object => ObjectValue?.ToString() ?? "null",
            TdValueType.Function => "<function>",
            TdValueType.NativeFunction => "<native>",
            TdValueType.Vector2 => $"Vector2({VecX}, {VecY})",
            TdValueType.Rect => $"Rect({RectX}, {RectY}, {RectW}, {RectH})",
            TdValueType.Color => $"#{R:X2}{G:X2}{B:X2}",
            TdValueType.Sprite => $"Sprite({SpritePath})",
            _ => "?"
        };
    }
}
