using System.Runtime.InteropServices;
using TdSharp.Core;
using TdSharp.CppCompiler;

namespace TdSharp.Engine;

public class CppNativeBridge
{
    private readonly CppCompiler.CppCompiler _compiler;
    private readonly Dictionary<string, IntPtr> _loadedDlls = new();
    private readonly string _projectDir;

    public CppNativeBridge(string projectDir, string mingwPath = null)
    {
        _projectDir = projectDir;
        _compiler = new CppCompiler.CppCompiler(mingwPath);
    }

    public void RegisterExtern(Interpreter interpreter)
    {
        interpreter.Globals.Define("extern", TdValue.NativeFuncVal(CallExtern));
    }

    private TdValue CallExtern(Interpreter interpreter, List<TdValue> args)
    {
        if (args.Count < 1 || args[0].Type != TdValueType.String)
        {
            Console.Error.WriteLine("extern() requires a string argument (path to .cpp file)");
            return TdValue.NilVal();
        }

        string cppPath = args[0].StrValue;
        string fullPath = Path.GetFullPath(Path.Combine(_projectDir, cppPath));
        if (!fullPath.StartsWith(_projectDir, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("extern() path must stay within the project directory.");
            return TdValue.NilVal();
        }
        if (!File.Exists(fullPath))
        {
            Console.Error.WriteLine($"extern() file not found: {fullPath}");
            return TdValue.NilVal();
        }

        string cppDir = Path.GetDirectoryName(fullPath) ?? _projectDir;
        string outputName = Path.GetFileNameWithoutExtension(cppPath);

        Console.WriteLine($"extern() compiling: {cppPath}...");
        var result = _compiler.Compile(cppDir, outputName);

        if (!result.Success)
        {
            Console.Error.WriteLine($"extern() compilation failed: {result.Output}");
            return TdValue.NilVal();
        }

        IntPtr dllHandle = LoadDll(result.DllPath);
        if (dllHandle == IntPtr.Zero)
        {
            Console.Error.WriteLine($"extern() failed to load DLL: {result.DllPath}");
            return TdValue.NilVal();
        }

        var proxy = new Dictionary<string, TdValue>();
        proxy["__dll"] = TdValue.StrVal(result.DllPath);
        proxy["__loaded"] = TdValue.BoolVal(true);

        var exportedNames = GetExportedFunctions(dllHandle);
        foreach (var name in exportedNames)
        {
            var nativeFunc = CreateNativeWrapper(dllHandle, name);
            proxy[name] = TdValue.NativeFuncVal(nativeFunc);
        }

        return TdValue.MapVal(proxy);
    }

    private IntPtr LoadDll(string dllPath)
    {
        if (_loadedDlls.TryGetValue(dllPath, out var existing))
            return existing;

        if (!NativeLibrary.TryLoad(dllPath, out var handle))
            return IntPtr.Zero;

        _loadedDlls[dllPath] = handle;
        return handle;
    }

    private static List<string> GetExportedFunctions(IntPtr dllHandle)
    {
        var names = new List<string>();
        try
        {
            var exports = NativeLibrary.GetExport(dllHandle, "tds_exports");
            if (exports != IntPtr.Zero)
            {
                var getExports = Marshal.GetDelegateForFunctionPointer<GetExportsDelegate>(exports);
                var countPtr = Marshal.AllocHGlobal(sizeof(int));
                try
                {
                    var namesPtr = getExports(out int count);
                    for (int i = 0; i < count; i++)
                    {
                        var namePtr = Marshal.ReadIntPtr(namesPtr, i * IntPtr.Size);
                        if (namePtr != IntPtr.Zero)
                            names.Add(Marshal.PtrToStringAnsi(namePtr) ?? "");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(countPtr);
                }
            }
        }
        catch { }
        return names;
    }

    private static Func<Interpreter, List<TdValue>, TdValue> CreateNativeWrapper(IntPtr dllHandle, string funcName)
    {
        return (interpreter, args) =>
        {
            try
            {
                var address = NativeLibrary.GetExport(dllHandle, funcName);
                if (address == IntPtr.Zero)
                    return TdValue.NilVal();

                var del = Marshal.GetDelegateForFunctionPointer<CppFunction>(address);
                var nativeArgs = args.Select(a => a.Type switch
                {
                    TdValueType.Number => (object)a.NumValue,
                    TdValueType.String => a.StrValue,
                    TdValueType.Bool => a.BoolValue,
                    _ => a.ToString()
                }).ToArray();

                var result = del(nativeArgs);

                return result switch
                {
                    double d => TdValue.NumVal(d),
                    int i => TdValue.NumVal(i),
                    string s => TdValue.StrVal(s),
                    bool b => TdValue.BoolVal(b),
                    null => TdValue.NilVal(),
                    _ => TdValue.StrVal(result.ToString() ?? "")
                };
            }
            catch
            {
                return TdValue.NilVal();
            }
        };
    }

    private delegate IntPtr GetExportsDelegate(out int count);
    private delegate object CppFunction(params object[] args);
}
