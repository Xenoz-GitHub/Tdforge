namespace TdSharp.Core;

public class Interpreter : Expr.IVisitor<TdValue>, Stmt.IVisitor<object>
{
    public Environment Globals { get; } = new();
    private Environment _environment;
    private readonly Dictionary<Expr, int> _locals = new();
    private readonly Random _random = new();
    public IGameEngine Engine { get; set; } = new ConsoleEngine();
    public Action<List<Stmt>> ReloadHandler { get; set; }

    // Game event callbacks
    public Action<double> TickCallback { get; set; }
    public Action PaintCallback { get; set; }
    public Action<string> PressedCallback { get; set; }
    public Action<string> ReleasedCallback { get; set; }
    public Action<string> TapCallback { get; set; }
    public Action<double, double> MoveCallback { get; set; }
    public Action<TdValue, TdValue> BumpCallback { get; set; }

    // Stored callable functions for events
    private TdFunction _tickFunc;
    private TdFunction _paintFunc;
    private TdFunction _pressedFunc;
    private TdFunction _releasedFunc;
    private TdFunction _tapFunc;
    private TdFunction _moveFunc;
    private TdFunction _bumpFunc;
    private TdFunction _startFunc;
    private List<Stmt> _program;
    private readonly HashSet<string> _importedFiles = new();

    public string BaseDirectory { get; set; } = ".";

    public Interpreter()
    {
        _environment = Globals;
        RegisterBuiltIns();
    }

    public void Interpret(List<Stmt> program)
    {
        _program = program;
        try
        {
            foreach (var stmt in program)
            {
                if (stmt is Stmt.Function funcDecl)
                {
                    var func = new TdFunction(funcDecl.Name.Lexeme,
                        funcDecl.Parameters.Select(p => p.Lexeme).ToList(),
                        funcDecl.Body, Globals);
                    Globals.Define(funcDecl.Name.Lexeme, TdValue.FuncVal(func));

                    // Register event callbacks
                    switch (funcDecl.Name.Lexeme)
                    {
                        case "tick": _tickFunc = func; break;
                        case "paint": _paintFunc = func; break;
                        case "pressed": _pressedFunc = func; break;
                        case "released": _releasedFunc = func; break;
                        case "tap": _tapFunc = func; break;
                        case "move": _moveFunc = func; break;
                        case "bump": _bumpFunc = func; break;
                        case "start": _startFunc = func; break;
                    }
                }
                else if (stmt is Stmt.Class classDecl)
                {
                    ExecuteClassDecl(classDecl);
                }
                else if (stmt is Stmt.Import importStmt)
                {
                    VisitImportStmt(importStmt);
                }
            }

            // Call start() if defined
            if (_startFunc != null)
            {
                var env = new Environment(Globals);
                var closure = new Environment(_startFunc.Closure);
                _environment = closure;
                ExecuteBlock(_startFunc.Body, new Environment(_startFunc.Closure));
                _environment = Globals;
            }

            // For console mode: execute non-function/non-class statements in order
            bool isGameMode = _tickFunc != null || _paintFunc != null;
            if (!isGameMode)
            {
                foreach (var stmt in program)
                {
                    if (stmt is Stmt.Expression || stmt is Stmt.Var || stmt is Stmt.Const ||
                        stmt is Stmt.If || stmt is Stmt.While || stmt is Stmt.For ||
                        stmt is Stmt.ForEach || stmt is Stmt.Match || stmt is Stmt.Import ||
                        stmt is Stmt.Return || stmt is Stmt.Break || stmt is Stmt.Continue ||
                        stmt is Stmt.Block)
                    {
                        Execute(stmt);
                    }
                }
            }
        }
        catch (RuntimeException e)
        {
            Console.Error.WriteLine($"Td# Runtime Error at line {e.Line}: {e.Message}");
        }
    }

    // Called each frame by game engine
    public void Tick(double dt)
    {
        if (_tickFunc != null)
        {
            var env = new Environment(_tickFunc.Closure);
            env.Define("dt", TdValue.NumVal(dt));
            _environment = env;
            ExecuteBlock(_tickFunc.Body, env);
            _environment = Globals;
        }
    }

    public void Paint()
    {
        if (_paintFunc != null)
        {
            var env = new Environment(_paintFunc.Closure);
            _environment = env;
            ExecuteBlock(_paintFunc.Body, env);
            _environment = Globals;
        }
    }

    public void OnPressed(string key)
    {
        if (_pressedFunc != null)
        {
            var env = new Environment(_pressedFunc.Closure);
            env.Define("key", TdValue.StrVal(key));
            _environment = env;
            ExecuteBlock(_pressedFunc.Body, env);
            _environment = Globals;
        }
    }

    public void OnReleased(string key)
    {
        if (_releasedFunc != null)
        {
            var env = new Environment(_releasedFunc.Closure);
            env.Define("key", TdValue.StrVal(key));
            _environment = env;
            ExecuteBlock(_releasedFunc.Body, env);
            _environment = Globals;
        }
    }

    public void OnTap(string button)
    {
        if (_tapFunc != null)
        {
            var env = new Environment(_tapFunc.Closure);
            env.Define("button", TdValue.StrVal(button));
            _environment = env;
            ExecuteBlock(_tapFunc.Body, env);
            _environment = Globals;
        }
    }

    public void OnMove(double x, double y)
    {
        if (_moveFunc != null)
        {
            var env = new Environment(_moveFunc.Closure);
            env.Define("x", TdValue.NumVal(x));
            env.Define("y", TdValue.NumVal(y));
            _environment = env;
            ExecuteBlock(_moveFunc.Body, env);
            _environment = Globals;
        }
    }

    public void OnBump(TdValue objA, TdValue objB)
    {
        if (_bumpFunc != null)
        {
            var env = new Environment(_bumpFunc.Closure);
            env.Define("obj_a", objA);
            env.Define("obj_b", objB);
            _environment = env;
            ExecuteBlock(_bumpFunc.Body, env);
            _environment = Globals;
        }
    }

    public bool IsGameMode => _tickFunc != null || _paintFunc != null;

    // ==================== BUILT-IN REGISTRATION ====================

    private void RegisterBuiltIns()
    {
        // Console output
        Globals.Define("say", TdValue.NativeFuncVal((interpreter, args) =>
        {
            string msg = string.Join(" ", args.Select(a => a.ToString()));
            Console.WriteLine("> " + msg);
            return TdValue.NilVal();
        }));

        // Helper for argument validation
        static void RequireArgs(List<TdValue> args, int count, string name)
        {
            if (args.Count < count)
                throw new RuntimeException($"'{name}' requires at least {count} argument(s), got {args.Count}.", 0);
        }

        // Math
        Globals.Define("roll", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "roll");
            double min = args[0].NumValue;
            double max = args[1].NumValue;
            return TdValue.NumVal(min + _random.NextDouble() * (max - min));
        }));

        Globals.Define("abs", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "abs");
            return TdValue.NumVal(Math.Abs(args[0].NumValue));
        }));

        Globals.Define("min", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "min");
            return TdValue.NumVal(Math.Min(args[0].NumValue, args[1].NumValue));
        }));

        Globals.Define("max", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "max");
            return TdValue.NumVal(Math.Max(args[0].NumValue, args[1].NumValue));
        }));

        Globals.Define("clamp", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 3, "clamp");
            double val = args[0].NumValue, min = args[1].NumValue, max = args[2].NumValue;
            return TdValue.NumVal(Math.Clamp(val, min, max));
        }));

        Globals.Define("dist", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 4, "dist");
            double dx = args[0].NumValue - args[2].NumValue;
            double dy = args[1].NumValue - args[3].NumValue;
            return TdValue.NumVal(Math.Sqrt(dx * dx + dy * dy));
        }));

        Globals.Define("smooth", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 3, "smooth");
            return TdValue.NumVal(args[0].NumValue + (args[1].NumValue - args[0].NumValue) * args[2].NumValue);
        }));

        // Length operator (#)
        Globals.Define("__len", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "__len");
            var val = args[0];
            if (val.Type == TdValueType.Array)
                return TdValue.NumVal(val.ArrayValue.Count);
            if (val.Type == TdValueType.Map)
                return TdValue.NumVal(val.MapValue.Count);
            if (val.Type == TdValueType.String)
                return TdValue.NumVal(val.StrValue.Length);
            return TdValue.NumVal(0);
        }));

        // Game graphics (passthrough to engine)
        Globals.Define("clear", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "clear");
            var color = args[0];
            interpreter.Engine.Clear(color.R, color.G, color.B, color.A);
            return TdValue.NilVal();
        }));

        Globals.Define("show", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "show");
            if (args[0].Type == TdValueType.Sprite)
            {
                double x = args[1].NumValue, y = args[2].NumValue;
                double rot = 0, sx = 1, sy = 1;
                TdValue tint = TdValue.ColorVal(255, 255, 255, 255);
                if (args.Count > 3) rot = args[3].NumValue;
                if (args.Count > 4) sx = args[4].NumValue;
                if (args.Count > 5) sy = args[5].NumValue;
                if (args.Count > 6) tint = args[6];
                interpreter.Engine.ShowSprite(args[0].SpritePath, x, y, rot, sx, sy, tint);
            }
            else if (args[0].Type == TdValueType.String)
            {
                double x = args[1].NumValue, y = args[2].NumValue;
                double size = args[3].NumValue;
                var color = args[4];
                interpreter.Engine.ShowText(args[0].StrValue, x, y, size, color.R, color.G, color.B, color.A);
            }
            return TdValue.NilVal();
        }));

        Globals.Define("rect", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 5, "rect");
            var c = args[4];
            interpreter.Engine.DrawRect(args[0].NumValue, args[1].NumValue,
                args[2].NumValue, args[3].NumValue, c.R, c.G, c.B, c.A);
            return TdValue.NilVal();
        }));

        Globals.Define("circle", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 4, "circle");
            var c = args[3];
            interpreter.Engine.DrawCircle(args[0].NumValue, args[1].NumValue,
                args[2].NumValue, c.R, c.G, c.B, c.A);
            return TdValue.NilVal();
        }));

        Globals.Define("line", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 5, "line");
            var c = args[4];
            interpreter.Engine.DrawLine(args[0].NumValue, args[1].NumValue,
                args[2].NumValue, args[3].NumValue, c.R, c.G, c.B, c.A);
            return TdValue.NilVal();
        }));

        Globals.Define("tilemap", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 3, "tilemap");
            interpreter.Engine.DrawTilemap(args[0], args[1].NumValue, args[2].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("holding", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "holding");
            return TdValue.BoolVal(interpreter.Engine.Holding(args[0].StrValue));
        }));

        Globals.Define("tap", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "tap");
            return TdValue.BoolVal(interpreter.Engine.MouseTap(args[0].StrValue));
        }));

        Globals.Define("mouse_x", TdValue.NativeFuncVal((interpreter, args) =>
            TdValue.NumVal(interpreter.Engine.MouseX())));

        Globals.Define("mouse_y", TdValue.NativeFuncVal((interpreter, args) =>
            TdValue.NumVal(interpreter.Engine.MouseY())));

        Globals.Define("go_to", TdValue.NativeFuncVal((interpreter, args) =>
        {
            interpreter.Engine.Reload();
            return TdValue.NilVal();
        }));

        Globals.Define("quit", TdValue.NativeFuncVal((interpreter, args) =>
        {
            System.Environment.Exit(0);
            return TdValue.NilVal();
        }));

        Globals.Define("spawn", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "spawn");
            var original = args[0];
            if (original.Type == TdValueType.Object)
            {
                var clone = new TdObject(original.ObjectValue.ClassName);
                foreach (var kv in original.ObjectValue.Fields)
                    clone.Fields[kv.Key] = kv.Value;
                foreach (var kv in original.ObjectValue.Methods)
                    clone.Methods[kv.Key] = kv.Value;
                return TdValue.ObjVal(clone);
            }
            return TdValue.NilVal();
        }));

        Globals.Define("remove", TdValue.NativeFuncVal((interpreter, args) => TdValue.NilVal()));

        // SFX
        Globals.Define("play_sound", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "play_sound");
            interpreter.Engine.PlaySound(args[0].StrValue);
            return TdValue.NilVal();
        }));

        Globals.Define("play_music", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "play_music");
            interpreter.Engine.PlayMusic(args[0].StrValue);
            return TdValue.NilVal();
        }));

        Globals.Define("stop_music", TdValue.NativeFuncVal((interpreter, args) =>
        {
            interpreter.Engine.StopMusic();
            return TdValue.NilVal();
        }));

        Globals.Define("set_sound_volume", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "set_sound_volume");
            interpreter.Engine.SetSoundVolume(args[0].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("set_music_volume", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "set_music_volume");
            interpreter.Engine.SetMusicVolume(args[0].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("is_playing", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "is_playing");
            return TdValue.BoolVal(interpreter.Engine.IsPlaying(args[0].StrValue));
        }));

        // VFX
        Globals.Define("emit", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 4, "emit");
            interpreter.Engine.Emit(args[0].StrValue, args[1].NumValue, args[2].NumValue, (int)args[3].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("shake", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "shake");
            interpreter.Engine.Shake(args[0].NumValue, args[1].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("fade_in", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "fade_in");
            var c = args[1];
            interpreter.Engine.FadeIn(args[0].NumValue, c.R, c.G, c.B, c.A);
            return TdValue.NilVal();
        }));

        Globals.Define("fade_out", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "fade_out");
            var c = args[1];
            interpreter.Engine.FadeOut(args[0].NumValue, c.R, c.G, c.B, c.A);
            return TdValue.NilVal();
        }));

        Globals.Define("flash", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "flash");
            var c = args[0];
            interpreter.Engine.Flash(c.R, c.G, c.B, c.A, args[1].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("tween", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 4, "tween");
            interpreter.Engine.Tween(args[0], args[1].StrValue, args[2].NumValue, args[3].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("trail", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "trail");
            interpreter.Engine.Trail(args[0].SpritePath, args[1].BoolValue);
            return TdValue.NilVal();
        }));

        Globals.Define("tint", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "tint");
            interpreter.Engine.ShowSprite(args[0].SpritePath, 0, 0, 0, 1, 1, args[1]);
            return TdValue.NilVal();
        }));

        // Extended
        Globals.Define("gamepad_held", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "gamepad_held");
            return TdValue.BoolVal(interpreter.Engine.GamepadHeld(args[0].StrValue));
        }));

        Globals.Define("gamepad_axis", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "gamepad_axis");
            return TdValue.NumVal(interpreter.Engine.GamepadAxis(args[0].StrValue));
        }));

        Globals.Define("after", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "after");
            interpreter.Engine.Timer(args[0].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("save_data", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "save_data");
            interpreter.Engine.SaveData(args[0].StrValue, args[1]);
            return TdValue.NilVal();
        }));

        Globals.Define("load_data", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "load_data");
            return interpreter.Engine.LoadData(args[0].StrValue);
        }));

        Globals.Define("timer", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "timer");
            return TdValue.NumVal(interpreter.Engine.Timer(args[0].NumValue));
        }));

        Globals.Define("timer_elapsed", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "timer_elapsed");
            return TdValue.BoolVal(interpreter.Engine.TimerElapsed(args[0].NumValue));
        }));

        // Animation
        Globals.Define("animate", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 3, "animate");
            interpreter.Engine.Animate(args[0].SpritePath, args[1].StrValue, args[2].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("add_animation", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 3, "add_animation");
            if (args[1].Type != TdValueType.Array)
                throw new RuntimeException("'add_animation' requires an array as second argument.", 0);
            var frames = args[1].ArrayValue.Select(v => (int)v.NumValue).ToList();
            interpreter.Engine.AddAnimation(args[0].StrValue, frames, args[2].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("get_frame", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "get_frame");
            return TdValue.NumVal(interpreter.Engine.GetFrame(args[0].SpritePath));
        }));

        Globals.Define("set_frame", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "set_frame");
            interpreter.Engine.SetFrame(args[0].SpritePath, (int)args[1].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("animation_finished", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "animation_finished");
            return TdValue.BoolVal(interpreter.Engine.AnimationFinished(args[0].SpritePath));
        }));

        // Camera
        Globals.Define("camera_follow", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "camera_follow");
            interpreter.Engine.CameraFollow(args[0], args[1].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("camera_zoom", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "camera_zoom");
            interpreter.Engine.CameraZoom(args[0].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("camera_rotate", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "camera_rotate");
            interpreter.Engine.CameraRotate(args[0].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("camera_bounds", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "camera_bounds");
            var r = args[0];
            interpreter.Engine.CameraBounds(r.RectX, r.RectY, r.RectW, r.RectH);
            return TdValue.NilVal();
        }));

        Globals.Define("world_to_screen", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "world_to_screen");
            return interpreter.Engine.WorldToScreen(args[0].NumValue, args[1].NumValue);
        }));

        // Debug
        Globals.Define("draw_hitbox", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "draw_hitbox");
            interpreter.Engine.DrawHitbox(args[0].BoolValue);
            return TdValue.NilVal();
        }));

        Globals.Define("show_fps", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "show_fps");
            interpreter.Engine.ShowFps(args[0].BoolValue);
            return TdValue.NilVal();
        }));

        Globals.Define("profile", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "profile");
            interpreter.Engine.Profile(args[0].StrValue);
            return TdValue.NilVal();
        }));

        Globals.Define("inspect", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "inspect");
            Console.WriteLine("> " + args[0].ToString());
            return TdValue.NilVal();
        }));

        Globals.Define("breakpoint", TdValue.NativeFuncVal((interpreter, args) =>
        {
            interpreter.Engine.Breakpoint();
            return TdValue.NilVal();
        }));

        Globals.Define("step_frame", TdValue.NativeFuncVal((interpreter, args) =>
        {
            interpreter.Engine.StepFrame();
            return TdValue.NilVal();
        }));

        // Procedural
        Globals.Define("noise", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "noise");
            return TdValue.NumVal(interpreter.Engine.Noise(args[0].NumValue, args[1].NumValue));
        }));

        Globals.Define("grid", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "grid");
            return interpreter.Engine.Grid((int)args[0].NumValue, (int)args[1].NumValue);
        }));

        Globals.Define("fill_rect_map", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 6, "fill_rect_map");
            interpreter.Engine.FillRectMap(args[0], (int)args[1].NumValue, (int)args[2].NumValue,
                (int)args[3].NumValue, (int)args[4].NumValue, (int)args[5].NumValue);
            return TdValue.NilVal();
        }));

        Globals.Define("find_path", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 4, "find_path");
            return interpreter.Engine.FindPath(args[0].NumValue, args[1].NumValue,
                args[2].NumValue, args[3].NumValue);
        }));

        Globals.Define("path_length", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "path_length");
            return TdValue.NumVal(interpreter.Engine.PathLength(args[0]));
        }));

        // Dev tools
        Globals.Define("reload", TdValue.NativeFuncVal((interpreter, args) =>
        {
            interpreter.Engine.Reload();
            return TdValue.NilVal();
        }));

        Globals.Define("record", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "record");
            interpreter.Engine.Record(args[0].StrValue);
            return TdValue.NilVal();
        }));

        Globals.Define("playback", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "playback");
            interpreter.Engine.Playback(args[0].StrValue);
            return TdValue.NilVal();
        }));

        Globals.Define("time_scale", TdValue.NativeFuncVal((interpreter, args) =>
            TdValue.NumVal(interpreter.Engine.TimeScale())));

        // File I/O
        Globals.Define("read_file", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "read_file");
            string path = args[0].StrValue;
            string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(interpreter.BaseDirectory, path));
            if (!System.IO.File.Exists(fullPath))
                throw new RuntimeException($"File not found: '{path}'.", 0);
            return TdValue.StrVal(System.IO.File.ReadAllText(fullPath));
        }));

        Globals.Define("write_file", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "write_file");
            string path = args[0].StrValue;
            string content = args[1].ToString();
            string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(interpreter.BaseDirectory, path));
            System.IO.File.WriteAllText(fullPath, content);
            return TdValue.NilVal();
        }));

        Globals.Define("append_file", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "append_file");
            string path = args[0].StrValue;
            string content = args[1].ToString();
            string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(interpreter.BaseDirectory, path));
            System.IO.File.AppendAllText(fullPath, content);
            return TdValue.NilVal();
        }));

        Globals.Define("file_exists", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "file_exists");
            string path = args[0].StrValue;
            string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(interpreter.BaseDirectory, path));
            return TdValue.BoolVal(System.IO.File.Exists(fullPath));
        }));

        // Built-in type constructors
        Globals.Define("Vector2", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 2, "Vector2");
            return TdValue.Vec2Val(args[0].NumValue, args[1].NumValue);
        }));

        Globals.Define("Rect", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 4, "Rect");
            return TdValue.RectVal(args[0].NumValue, args[1].NumValue, args[2].NumValue, args[3].NumValue);
        }));

        Globals.Define("Color", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 3, "Color");
            return TdValue.ColorVal((byte)args[0].NumValue, (byte)args[1].NumValue,
                (byte)args[2].NumValue, args.Count > 3 ? (byte)args[3].NumValue : (byte)255);
        }));

        Globals.Define("Sprite", TdValue.NativeFuncVal((interpreter, args) =>
        {
            RequireArgs(args, 1, "Sprite");
            return TdValue.SpriteVal(args[0].StrValue);
        }));
    }

    // ==================== STATEMENT EXECUTION ====================

    public object Execute(Stmt stmt)
    {
        return stmt.Accept(this);
    }

    public void ExecuteBlock(List<Stmt> statements, Environment env)
    {
        var previous = _environment;
        try
        {
            _environment = env;
            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
        }
        finally
        {
            _environment = previous;
        }
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object VisitVarStmt(Stmt.Var stmt)
    {
        TdValue value = TdValue.NilVal();
        if (stmt.Initializer != null)
            value = Evaluate(stmt.Initializer);
        _environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public object VisitConstStmt(Stmt.Const stmt)
    {
        TdValue value = Evaluate(stmt.Initializer);
        _environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public object VisitIfStmt(Stmt.If stmt)
    {
        if (Evaluate(stmt.Condition).IsTruthy())
        {
            ExecuteBlock(stmt.ThenBranch, new Environment(_environment));
        }
        else
        {
            bool matched = false;
            foreach (var elif in stmt.ElifBranches)
            {
                if (elif is Stmt.If elifStmt && Evaluate(elifStmt.Condition).IsTruthy())
                {
                    ExecuteBlock(elifStmt.ThenBranch, new Environment(_environment));
                    matched = true;
                    break;
                }
            }
            if (!matched && stmt.ElseBranch.Count > 0)
                ExecuteBlock(stmt.ElseBranch, new Environment(_environment));
        }
        return null;
    }

    public object VisitWhileStmt(Stmt.While stmt)
    {
        while (Evaluate(stmt.Condition).IsTruthy())
        {
            try
            {
                ExecuteBlock(stmt.Body, new Environment(_environment));
            }
            catch (BreakException) { break; }
            catch (ContinueException) { continue; }
        }
        return null;
    }

    public object VisitForStmt(Stmt.For stmt)
    {
        double start = Evaluate(stmt.Start).NumValue;
        double end = Evaluate(stmt.End).NumValue;
        long maxIterations = 100_000_000;
        long iterCount = 0;
        for (double i = start; i <= end; i++)
        {
            var env = new Environment(_environment);
            env.Define(stmt.Variable.Lexeme, TdValue.NumVal(i));
            try
            {
                ExecuteBlock(stmt.Body, env);
            }
            catch (BreakException) { break; }
            catch (ContinueException) { continue; }
            if (++iterCount > maxIterations)
                throw new RuntimeException($"Loop exceeded {maxIterations} iterations (possible infinite loop).", stmt.Variable.Line);
        }
        return null;
    }

    public object VisitForEachStmt(Stmt.ForEach stmt)
    {
        var collection = Evaluate(stmt.Collection);
        if (collection.Type == TdValueType.Array)
        {
            foreach (var item in collection.ArrayValue)
            {
                var env = new Environment(_environment);
                env.Define(stmt.Variable.Lexeme, item);
                try
                {
                    ExecuteBlock(stmt.Body, env);
                }
                catch (BreakException) { break; }
                catch (ContinueException) { continue; }
            }
        }
        else if (collection.Type == TdValueType.Map)
        {
            foreach (var kv in collection.MapValue)
            {
                var env = new Environment(_environment);
                env.Define(stmt.Variable.Lexeme, TdValue.StrVal(kv.Key));
                try
                {
                    ExecuteBlock(stmt.Body, env);
                }
                catch (BreakException) { break; }
                catch (ContinueException) { continue; }
            }
        }
        return null;
    }

    public object VisitMatchStmt(Stmt.Match stmt)
    {
        var value = Evaluate(stmt.Value);
        bool matched = false;
        foreach (var (pattern, body) in stmt.Cases)
        {
            var patternVal = Evaluate(pattern);
            if (ValuesEqual(value, patternVal))
            {
                ExecuteBlock(body, new Environment(_environment));
                matched = true;
                break;
            }
        }
        if (!matched && stmt.ElseBody.Count > 0)
            ExecuteBlock(stmt.ElseBody, new Environment(_environment));
        return null;
    }

    private bool ValuesEqual(TdValue a, TdValue b)
    {
        if (a.Type != b.Type) return false;
        switch (a.Type)
        {
            case TdValueType.Nil: return true;
            case TdValueType.Bool: return a.BoolValue == b.BoolValue;
            case TdValueType.Number: return Math.Abs(a.NumValue - b.NumValue) < 0.0001;
            case TdValueType.String: return a.StrValue == b.StrValue;
            case TdValueType.Array: return a.ArrayValue == b.ArrayValue;
            case TdValueType.Map: return a.MapValue == b.MapValue;
            case TdValueType.Object: return a.ObjectValue == b.ObjectValue;
            default: return false;
        }
    }

    public object VisitFunctionStmt(Stmt.Function stmt)
    {
        var func = new TdFunction(stmt.Name.Lexeme,
            stmt.Parameters.Select(p => p.Lexeme).ToList(),
            stmt.Body, _environment);
        _environment.Define(stmt.Name.Lexeme, TdValue.FuncVal(func));
        return null;
    }

    public void ExecuteClassDecl(Stmt.Class stmt)
    {
        TdObject meta = null;
        if (stmt.InheritsFrom != null)
        {
            var parent = Globals.Get(new Token(TokenType.Identifier, stmt.InheritsFrom, null, stmt.Name.Line));
            if (parent.Type == TdValueType.Object)
                meta = parent.ObjectValue;
        }

        var classObj = new TdObject(stmt.Name.Lexeme);

        if (meta != null)
        {
            foreach (var kv in meta.Methods)
                classObj.Methods[kv.Key] = kv.Value;
        }

        foreach (var method in stmt.Methods)
        {
            var func = new TdFunction(method.Name.Lexeme,
                method.Parameters.Select(p => p.Lexeme).ToList(),
                method.Body, _environment, false, stmt.Name.Lexeme);
            classObj.Methods[method.Name.Lexeme] = func;
        }

        if (stmt.Constructor != null)
        {
            var ctor = new TdFunction("constructor",
                stmt.Constructor.Parameters.Select(p => p.Lexeme).ToList(),
                stmt.Constructor.Body, _environment, true, stmt.Name.Lexeme);
            classObj.Methods["constructor"] = ctor;
        }

        Globals.Define(stmt.Name.Lexeme, TdValue.ObjVal(classObj));
    }

    public object VisitClassStmt(Stmt.Class stmt)
    {
        ExecuteClassDecl(stmt);
        return null;
    }

    public object VisitReturnStmt(Stmt.Return stmt)
    {
        TdValue value = TdValue.NilVal();
        if (stmt.Value != null)
            value = Evaluate(stmt.Value);
        throw new ReturnException(value);
    }

    public object VisitBreakStmt(Stmt.Break stmt)
    {
        throw new BreakException();
    }

    public object VisitContinueStmt(Stmt.Continue stmt)
    {
        throw new ContinueException();
    }

    public object VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(_environment));
        return null;
    }

    public object VisitImportStmt(Stmt.Import stmt)
    {
        var pathVal = Evaluate(stmt.Path);
        if (pathVal.Type != TdValueType.String)
            throw new RuntimeException("Import path must be a string.", stmt.Keyword.Line);

        string relativePath = pathVal.StrValue;
        string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(BaseDirectory, relativePath));

        if (!_importedFiles.Add(fullPath))
            return null; // already imported

        if (!System.IO.File.Exists(fullPath))
            throw new RuntimeException($"Import file not found: '{relativePath}'.", stmt.Keyword.Line);

        string source = System.IO.File.ReadAllText(fullPath);
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        var statements = parser.Parse();

        var savedDir = BaseDirectory;
        BaseDirectory = System.IO.Path.GetDirectoryName(fullPath);
        try
        {
            foreach (var s in statements)
                Execute(s);
        }
        finally
        {
            BaseDirectory = savedDir;
        }

        return null;
    }

    // ==================== EXPRESSION EVALUATION ====================

    public TdValue Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    public TdValue VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        switch (expr.Op.Type)
        {
            case TokenType.Plus:
                if (left.Type == TdValueType.Number && right.Type == TdValueType.Number)
                    return TdValue.NumVal(left.NumValue + right.NumValue);
                if (left.Type == TdValueType.String || right.Type == TdValueType.String)
                    return TdValue.StrVal(left.ToString() + right.ToString());
                throw new RuntimeException("Operands must be numbers or strings for '+'.", expr.Op.Line);

            case TokenType.Minus:
                CheckNumberOperands(expr.Op, left, right);
                return TdValue.NumVal(left.NumValue - right.NumValue);

            case TokenType.Star:
                CheckNumberOperands(expr.Op, left, right);
                return TdValue.NumVal(left.NumValue * right.NumValue);

            case TokenType.Slash:
                CheckNumberOperands(expr.Op, left, right);
                if (right.NumValue == 0) throw new RuntimeException("Division by zero.", expr.Op.Line);
                return TdValue.NumVal(left.NumValue / right.NumValue);

            case TokenType.Percent:
                CheckNumberOperands(expr.Op, left, right);
                return TdValue.NumVal(left.NumValue % right.NumValue);

            case TokenType.Caret:
                CheckNumberOperands(expr.Op, left, right);
                return TdValue.NumVal(Math.Pow(left.NumValue, right.NumValue));

            case TokenType.DotDot:
                return TdValue.StrVal(left.ToString() + right.ToString());

            // Comparison
            case TokenType.EqualEqual:
                return TdValue.BoolVal(ValuesEqual(left, right));

            case TokenType.BangEqual:
                return TdValue.BoolVal(!ValuesEqual(left, right));

            case TokenType.Greater:
                CheckNumberOperands(expr.Op, left, right);
                return TdValue.BoolVal(left.NumValue > right.NumValue);

            case TokenType.GreaterEqual:
                CheckNumberOperands(expr.Op, left, right);
                return TdValue.BoolVal(left.NumValue >= right.NumValue);

            case TokenType.Less:
                CheckNumberOperands(expr.Op, left, right);
                return TdValue.BoolVal(left.NumValue < right.NumValue);

            case TokenType.LessEqual:
                CheckNumberOperands(expr.Op, left, right);
                return TdValue.BoolVal(left.NumValue <= right.NumValue);

            default:
                throw new RuntimeException($"Unknown operator '{expr.Op.Lexeme}'.", expr.Op.Line);
        }
    }

    public TdValue VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);
        switch (expr.Op.Type)
        {
            case TokenType.Minus:
                CheckNumberOperand(expr.Op, right);
                return TdValue.NumVal(-right.NumValue);
            case TokenType.Not:
                return TdValue.BoolVal(!right.IsTruthy());
            case TokenType.Hash:
                if (right.Type == TdValueType.Array)
                    return TdValue.NumVal(right.ArrayValue.Count);
                if (right.Type == TdValueType.Map)
                    return TdValue.NumVal(right.MapValue.Count);
                if (right.Type == TdValueType.String)
                    return TdValue.NumVal(right.StrValue.Length);
                return TdValue.NumVal(0);
            default:
                throw new RuntimeException($"Unknown unary operator '{expr.Op.Lexeme}'.", expr.Op.Line);
        }
    }

    public TdValue VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public TdValue VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public TdValue VisitVariableExpr(Expr.Variable expr)
    {
        return _environment.Get(expr.Name);
    }

    public TdValue VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);
        _environment.Assign(expr.Name, value);
        return value;
    }

    public TdValue VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);
        if (expr.Op.Type == TokenType.Or)
        {
            if (left.IsTruthy()) return left;
        }
        else
        {
            if (!left.IsTruthy()) return left;
        }
        return Evaluate(expr.Right);
    }

    public TdValue VisitCallExpr(Expr.Call expr)
    {
        var callee = Evaluate(expr.Callee);
        var args = expr.Arguments.Select(Evaluate).ToList();

        if (callee.Type == TdValueType.NativeFunction)
        {
            return callee.NativeFunc(this, args);
        }
        else if (callee.Type == TdValueType.Function)
        {
            var func = callee.FunctionValue;
            if (args.Count != func.Arity)
                throw new RuntimeException($"Expected {func.Arity} arguments but got {args.Count}.", expr.Paren.Line);

            var env = new Environment(func.Closure);
            for (int i = 0; i < func.Parameters.Count; i++)
                env.Define(func.Parameters[i], i < args.Count ? args[i] : TdValue.NilVal());

            // Handle default parameters - need the function definition for that
            // For now, undefined params remain nil

            try
            {
                ExecuteBlock(func.Body, env);
            }
            catch (ReturnException ret)
            {
                return ret.Value;
            }
            return TdValue.NilVal();
        }
        else if (callee.Type == TdValueType.Object)
        {
            // Class instantiation
            var classObj = callee.ObjectValue;
            var instance = new TdObject(classObj.ClassName);

            // Copy methods
            foreach (var kv in classObj.Methods)
                instance.Methods[kv.Key] = kv.Value;

            // Call constructor with 'this' bound
            if (instance.Methods.TryGetValue("constructor", out var ctor))
            {
                if (args.Count != ctor.Parameters.Count)
                    throw new RuntimeException($"Expected {ctor.Arity} constructor arguments but got {args.Count}.", expr.Paren.Line);

                var ctorEnv = new Environment(ctor.Closure);
                ctorEnv.Define("this", TdValue.ObjVal(instance));
                for (int i = 0; i < ctor.Parameters.Count; i++)
                    ctorEnv.Define(ctor.Parameters[i], i < args.Count ? args[i] : TdValue.NilVal());

                try
                {
                    ExecuteBlock(ctor.Body, ctorEnv);
                }
                catch (ReturnException) { }
            }

            return TdValue.ObjVal(instance);
        }
        else
        {
            throw new RuntimeException("Can only call functions and classes.", expr.Paren.Line);
        }
    }

    public TdValue VisitGetExpr(Expr.Get expr)
    {
        var obj = Evaluate(expr.Object);
        if (obj.Type == TdValueType.Object)
        {
            var instance = obj.ObjectValue;
            // Check fields first
            if (instance.Fields.TryGetValue(expr.Name.Lexeme, out var field))
                return field;
            // Then methods - bind this to instance
            if (instance.Methods.TryGetValue(expr.Name.Lexeme, out var method))
            {
                // Create a bound function with 'this' in closure
                var boundEnv = new Environment(method.Closure);
                boundEnv.Define("this", obj);
                var bound = new TdFunction(method.Name, method.Parameters, method.Body, boundEnv);
                return TdValue.FuncVal(bound);
            }
            throw new RuntimeException($"Undefined property '{expr.Name.Lexeme}'.", expr.Name.Line);
        }

        if (obj.Type == TdValueType.Map)
        {
            if (obj.MapValue.TryGetValue(expr.Name.Lexeme, out var mapVal))
                return mapVal;
            throw new RuntimeException($"Key '{expr.Name.Lexeme}' not found in map.", expr.Name.Line);
        }

        if (obj.Type == TdValueType.Vector2)
        {
            if (expr.Name.Lexeme == "x") return TdValue.NumVal(obj.VecX);
            if (expr.Name.Lexeme == "y") return TdValue.NumVal(obj.VecY);
            throw new RuntimeException($"Vector2 has no property '{expr.Name.Lexeme}'.", expr.Name.Line);
        }

        if (obj.Type == TdValueType.Rect)
        {
            return expr.Name.Lexeme switch
            {
                "x" => TdValue.NumVal(obj.RectX),
                "y" => TdValue.NumVal(obj.RectY),
                "w" => TdValue.NumVal(obj.RectW),
                "h" => TdValue.NumVal(obj.RectH),
                "hits" => TdValue.NativeFuncVal((interpreter, args) =>
                {
                    var other = args[0];
                    if (other.Type != TdValueType.Rect)
                        throw new RuntimeException("Rect.hits() requires a Rect argument.", expr.Name.Line);
                    bool hit = obj.RectX < other.RectX + other.RectW &&
                               obj.RectX + obj.RectW > other.RectX &&
                               obj.RectY < other.RectY + other.RectH &&
                               obj.RectY + obj.RectH > other.RectY;
                    return TdValue.BoolVal(hit);
                }),
                _ => throw new RuntimeException($"Rect has no property '{expr.Name.Lexeme}'.", expr.Name.Line)
            };
        }

        if (obj.Type == TdValueType.Color)
        {
            return expr.Name.Lexeme switch
            {
                "r" => TdValue.NumVal(obj.R),
                "g" => TdValue.NumVal(obj.G),
                "b" => TdValue.NumVal(obj.B),
                "a" => TdValue.NumVal(obj.A),
                _ => throw new RuntimeException($"Color has no property '{expr.Name.Lexeme}'.", expr.Name.Line)
            };
        }

        if (obj.Type == TdValueType.Sprite)
        {
            if (expr.Name.Lexeme == "path") return TdValue.StrVal(obj.SpritePath);
            throw new RuntimeException($"Sprite has no property '{expr.Name.Lexeme}'.", expr.Name.Line);
        }

        if (obj.Type == TdValueType.Array)
        {
            if (expr.Name.Lexeme == "push")
            {
                return TdValue.NativeFuncVal((interpreter, args) =>
                {
                    obj.ArrayValue.Add(args[0]);
                    return TdValue.NilVal();
                });
            }
            if (expr.Name.Lexeme == "pop")
            {
                return TdValue.NativeFuncVal((interpreter, args) =>
                {
                    if (obj.ArrayValue.Count > 0)
                    {
                        var last = obj.ArrayValue[^1];
                        obj.ArrayValue.RemoveAt(obj.ArrayValue.Count - 1);
                        return last;
                    }
                    return TdValue.NilVal();
                });
            }
            throw new RuntimeException($"Array has no property '{expr.Name.Lexeme}'.", expr.Name.Line);
        }

        throw new RuntimeException($"Objects of type '{obj.Type}' have no properties.", expr.Name.Line);
    }

    public TdValue VisitSetExpr(Expr.Set expr)
    {
        var obj = Evaluate(expr.Object);
        if (obj.Type == TdValueType.Object)
        {
            var value = Evaluate(expr.Value);
            obj.ObjectValue.Fields[expr.Name.Lexeme] = value;
            return value;
        }
        throw new RuntimeException("Can only set properties on objects.", expr.Name.Line);
    }

    public TdValue VisitArrayExpr(Expr.ArrayExpr expr)
    {
        var elements = expr.Elements.Select(Evaluate).ToList();
        return TdValue.ArrVal(elements);
    }

    public TdValue VisitMapExpr(Expr.MapExpr expr)
    {
        var map = new Dictionary<string, TdValue>();
        foreach (var (key, valueExpr) in expr.Entries)
        {
            map[key.Lexeme] = Evaluate(valueExpr);
        }
        return TdValue.MapVal(map);
    }

    public TdValue VisitIndexExpr(Expr.Index expr)
    {
        var obj = Evaluate(expr.Object);
        var index = Evaluate(expr.IndexExpr);

        if (obj.Type == TdValueType.Array)
        {
            if (index.Type != TdValueType.Number)
                throw new RuntimeException("Array index must be a number.", expr.IndexExpr is Expr.Literal lit ? 0 : 0);
            int i = (int)index.NumValue;
            if (i < 0 || i >= obj.ArrayValue.Count)
                throw new RuntimeException($"Array index {i} out of bounds (size {obj.ArrayValue.Count}).", 0);
            return obj.ArrayValue[i];
        }

        if (obj.Type == TdValueType.Map)
        {
            string key = index.ToString();
            if (obj.MapValue.TryGetValue(key, out var val))
                return val;
            throw new RuntimeException($"Key '{key}' not found in map.", 0);
        }

        if (obj.Type == TdValueType.String)
        {
            if (index.Type != TdValueType.Number)
                throw new RuntimeException("String index must be a number.", 0);
            int i = (int)index.NumValue;
            if (i < 0 || i >= obj.StrValue.Length)
                throw new RuntimeException($"String index {i} out of bounds.", 0);
            return TdValue.StrVal(obj.StrValue[i].ToString());
        }

        throw new RuntimeException("Cannot index this type.", 0);
    }

    private void CheckNumberOperand(Token op, TdValue operand)
    {
        if (operand.Type == TdValueType.Number) return;
        throw new RuntimeException("Operand must be a number.", op.Line);
    }

    private void CheckNumberOperands(Token op, TdValue left, TdValue right)
    {
        if (left.Type == TdValueType.Number && right.Type == TdValueType.Number) return;
        throw new RuntimeException("Operands must be numbers.", op.Line);
    }
}
