using TdSharp.Core;
using Raylib_cs;
using System.Numerics;
using RaylibColor = Raylib_cs.Color;

namespace TdSharp.Engine;

public class GameEngine : IGameEngine, IDisposable
{
    private const int ScreenWidth = 640;
    private const int ScreenHeight = 480;
    private const string WindowTitle = "Td# Game";

    private Interpreter _interpreter;
    private bool _running;
    private bool _drawHitbox;
    private bool _showFps;
    private double _timeScale = 1.0;

    private Vector2 _cameraOffset;
    private float _cameraZoom = 1.0f;
    private float _cameraRotation;
    private Raylib_cs.Rectangle? _cameraBounds;
    private bool _cameraFollowing;
    private TdValue _followTarget;
    private float _followSmoothness;

    private float _shakeAmplitude;
    private float _shakeDuration;
    private float _shakeTimer;

    private readonly Dictionary<string, Texture2D> _textures = new();
    private readonly Dictionary<string, Sound> _sounds = new();
    private readonly Dictionary<string, Music> _musicTracks = new();
    private string _currentMusic;
    private bool _musicPlaying;
    private float _soundVolume = 1f;
    private float _musicVolume = 1f;

    private byte _flashR, _flashG, _flashB, _flashA;
    private double _flashTimer;
    private double _flashDuration;

    public GameEngine(Interpreter interpreter)
    {
        _interpreter = interpreter;
        RegisterEngineBuiltins();
    }

    private void RegisterEngineBuiltins()
    {
        var baseDir = _interpreter.BaseDirectory;
        var bridge = new CppNativeBridge(baseDir);
        bridge.RegisterExtern(_interpreter);
    }

    public void Run()
    {
        Raylib.SetConfigFlags(ConfigFlags.VSyncHint);
        Raylib.InitWindow(ScreenWidth, ScreenHeight, WindowTitle);
        Raylib.SetTargetFPS(60);
        Raylib.InitAudioDevice();
        try
        {
            _running = true;
            double previousTime = Raylib.GetTime();

            while (_running && !Raylib.WindowShouldClose())
            {
                double currentTime = Raylib.GetTime();
                double dt = (currentTime - previousTime) * _timeScale;
                previousTime = currentTime;

                if (_shakeTimer > 0)
                {
                    _shakeTimer -= (float)dt;
                    if (_shakeTimer <= 0) { _shakeTimer = 0; _shakeAmplitude = 0; }
                }

                if (_flashTimer > 0)
                {
                    _flashTimer -= dt;
                    if (_flashTimer < 0) _flashTimer = 0;
                }

                _interpreter.Tick(dt);

                Raylib.BeginDrawing();
                Raylib.ClearBackground(RaylibColor.Black);

                float shakeX = 0, shakeY = 0;
                if (_shakeTimer > 0)
                {
                    shakeX = (float)(_random.NextDouble() - 0.5) * _shakeAmplitude * 2;
                    shakeY = (float)(_random.NextDouble() - 0.5) * _shakeAmplitude * 2;
                }

                if (_cameraFollowing && _followTarget != null)
                    UpdateCameraFollow();

                Raylib.BeginMode2D(new Camera2D
                {
                    Target = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f),
                    Offset = new Vector2(ScreenWidth / 2f + shakeX, ScreenHeight / 2f + shakeY),
                    Rotation = _cameraRotation,
                    Zoom = _cameraZoom
                });

                _interpreter.Paint();

                Raylib.EndMode2D();

                if (_showFps)
                    Raylib.DrawFPS(10, 10);

                if (_drawHitbox)
                    DrawHitboxes();

                if (_flashTimer > 0)
                {
                    float alpha = (float)(_flashTimer / _flashDuration);
                    var flashColor = new RaylibColor(_flashR, _flashG, _flashB, (byte)(_flashA * alpha));
                    Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, flashColor);
                }

                Raylib.EndDrawing();

                if (_musicPlaying && _currentMusic != null && _musicTracks.TryGetValue(_currentMusic, out var music))
                    Raylib.UpdateMusicStream(music);
            }
        }
        finally
        {
            Cleanup();
            Raylib.CloseAudioDevice();
            Raylib.CloseWindow();
        }
    }

    private void UpdateCameraFollow()
    {
        if (_followTarget == null) return;
        double targetX = 0, targetY = 0;
        if (_followTarget.Type == TdValueType.Object)
        {
            if (_followTarget.ObjectValue.Fields.TryGetValue("x", out var xv))
                targetX = xv.NumValue;
            if (_followTarget.ObjectValue.Fields.TryGetValue("y", out var yv))
                targetY = yv.NumValue;
        }
        else if (_followTarget.Type == TdValueType.Vector2)
        {
            targetX = _followTarget.VecX;
            targetY = _followTarget.VecY;
        }

        float targetCamX = (float)targetX - ScreenWidth / 2f;
        float targetCamY = (float)targetY - ScreenHeight / 2f;

        if (_cameraBounds.HasValue)
        {
            var b = _cameraBounds.Value;
            targetCamX = b.Width > ScreenWidth
                ? Math.Clamp(targetCamX, b.X, b.X + b.Width - ScreenWidth)
                : b.X;
            targetCamY = b.Height > ScreenHeight
                ? Math.Clamp(targetCamY, b.Y, b.Y + b.Height - ScreenHeight)
                : b.Y;
        }

        _cameraOffset.X += (targetCamX - _cameraOffset.X) * _followSmoothness * 0.05f;
        _cameraOffset.Y += (targetCamY - _cameraOffset.Y) * _followSmoothness * 0.05f;
    }

    private void DrawHitboxes()
    {
        foreach (var kv in _interpreter.Globals.AllValues)
        {
            var val = kv.Value;
            if (val.Type == TdValueType.Object)
            {
                var obj = val.ObjectValue;
                if (obj.Fields.TryGetValue("x", out var xv) &&
                    obj.Fields.TryGetValue("y", out var yv))
                {
                    float w = 32, h = 32;
                    if (obj.Fields.TryGetValue("w", out var wv)) w = (float)wv.NumValue;
                    if (obj.Fields.TryGetValue("h", out var hv)) h = (float)hv.NumValue;
                    Raylib.DrawRectangleLines((int)xv.NumValue, (int)yv.NumValue, (int)w, (int)h, RaylibColor.Red);
                }
            }
        }
    }

    private RaylibColor MakeColor(double r, double g, double b, double a)
    {
        return new RaylibColor((byte)r, (byte)g, (byte)b, (byte)a);
    }

    private void Cleanup()
    {
        foreach (var tex in _textures.Values) Raylib.UnloadTexture(tex);
        foreach (var snd in _sounds.Values) Raylib.UnloadSound(snd);
        foreach (var mus in _musicTracks.Values) Raylib.UnloadMusicStream(mus);
        _textures.Clear(); _sounds.Clear(); _musicTracks.Clear();
    }

    public void Dispose() => Cleanup();

    private readonly Random _random = new();

    public bool IsActive => true;

    public void Clear(double r, double g, double b, double a)
    {
        Raylib.ClearBackground(MakeColor(r, g, b, a));
    }

    public void ShowSprite(string path, double x, double y, double rot, double scaleX, double scaleY, TdValue tint)
    {
        if (!_textures.TryGetValue(path, out var tex))
        {
            if (File.Exists(path))
                tex = Raylib.LoadTexture(path);
            else
            {
                string[] searchPaths = {
                    Path.Combine("sprites", path), Path.Combine("assets", path),
                    Path.Combine("images", path), path
                };
                foreach (var sp in searchPaths)
                {
                    if (File.Exists(sp)) { tex = Raylib.LoadTexture(sp); break; }
                }
            }
            if (tex.Id == 0) return;
            _textures[path] = tex;
        }

        var src = new Rectangle(0, 0, tex.Width, tex.Height);
        var dst = new Rectangle((float)x, (float)y, tex.Width * (float)scaleX, tex.Height * (float)scaleY);
        var origin = new Vector2(0, 0);
        Raylib.DrawTexturePro(tex, src, dst, origin, (float)rot, RaylibColor.White);
    }

    public void ShowText(string text, double x, double y, double size, double r, double g, double b, double a)
    {
        Raylib.DrawText(text, (int)x, (int)y, (int)size, MakeColor(r, g, b, a));
    }

    public void DrawRect(double x, double y, double w, double h, double r, double g, double b, double a)
    {
        Raylib.DrawRectangle((int)x, (int)y, (int)w, (int)h, MakeColor(r, g, b, a));
    }

    public void DrawCircle(double x, double y, double radius, double r, double g, double b, double a)
    {
        Raylib.DrawCircle((int)x, (int)y, (float)radius, MakeColor(r, g, b, a));
    }

    public void DrawLine(double x1, double y1, double x2, double y2, double r, double g, double b, double a)
    {
        Raylib.DrawLine((int)x1, (int)y1, (int)x2, (int)y2, MakeColor(r, g, b, a));
    }

    public void DrawTilemap(TdValue tilemapData, double camX, double camY)
    {
        if (tilemapData.Type != TdValueType.Array) return;
        for (int row = 0; row < tilemapData.ArrayValue.Count; row++)
        {
            var rowData = tilemapData.ArrayValue[row];
            if (rowData.Type != TdValueType.Array) continue;
            for (int col = 0; col < rowData.ArrayValue.Count; col++)
            {
                int tileId = (int)rowData.ArrayValue[col].NumValue;
                if (tileId > 0)
                {
                    int ts = 32;
                    Raylib.DrawRectangle(col * ts - (int)camX, row * ts - (int)camY, ts, ts, RaylibColor.Gray);
                    Raylib.DrawRectangleLines(col * ts - (int)camX, row * ts - (int)camY, ts, ts, RaylibColor.DarkGray);
                }
            }
        }
    }

    public bool Holding(string key)
    {
        var k = MapKey(key);
        return k.HasValue && Raylib.IsKeyDown(k.Value);
    }

    public bool MouseTap(string button)
    {
        return button == "left" ? Raylib.IsMouseButtonPressed(MouseButton.Left) :
               button == "right" ? Raylib.IsMouseButtonPressed(MouseButton.Right) : false;
    }

    public double MouseX() => Raylib.GetMouseX();
    public double MouseY() => Raylib.GetMouseY();

    public void PlaySound(string path)
    {
        if (!_sounds.TryGetValue(path, out var sound))
        {
            if (File.Exists(path))
            {
                sound = Raylib.LoadSound(path);
                _sounds[path] = sound;
            }
            else return;
        }
        Raylib.PlaySound(sound);
    }

    public void PlayMusic(string path)
    {
        if (_musicPlaying && _currentMusic != null && _musicTracks.TryGetValue(_currentMusic, out var oldMusic))
        {
            Raylib.StopMusicStream(oldMusic);
            Raylib.UnloadMusicStream(oldMusic);
            _musicTracks.Remove(_currentMusic);
        }

        if (!_musicTracks.TryGetValue(path, out var music))
        {
            if (File.Exists(path))
            {
                music = Raylib.LoadMusicStream(path);
                _musicTracks[path] = music;
            }
            else return;
        }
        _currentMusic = path;
        _musicPlaying = true;
        Raylib.PlayMusicStream(music);
    }

    public void StopMusic()
    {
        if (_currentMusic != null && _musicTracks.TryGetValue(_currentMusic, out var music))
        {
            Raylib.StopMusicStream(music);
            _currentMusic = null;
            _musicPlaying = false;
        }
    }

    public void SetSoundVolume(double vol)
    {
        _soundVolume = (float)vol;
        Raylib.SetMasterVolume(Math.Max(_soundVolume, _musicVolume));
    }
    public void SetMusicVolume(double vol)
    {
        _musicVolume = (float)vol;
        Raylib.SetMasterVolume(Math.Max(_soundVolume, _musicVolume));
    }
    public bool IsPlaying(string path) => _currentMusic == path && _musicPlaying;

    public void Emit(string particleType, double x, double y, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = (float)(_random.NextDouble() * 100 + 20);
            float px = (float)x + (float)Math.Cos(angle) * speed * 0.1f;
            float py = (float)y + (float)Math.Sin(angle) * speed * 0.1f;
            var color = particleType switch
            {
                "fire" => new RaylibColor { R = 255, G = (byte)_random.Next(100, 255), B = 0, A = 255 },
                "smoke" => new RaylibColor { R = 128, G = 128, B = 128, A = 200 },
                "rain" => new RaylibColor { R = 100, G = 150, B = 255, A = 200 },
                "sparkle" => new RaylibColor { R = 255, G = 255, B = (byte)_random.Next(100, 255), A = 255 },
                _ => RaylibColor.White
            };
            Raylib.DrawCircle((int)px, (int)py, _random.Next(2, 6), color);
        }
    }

    public void Shake(double amplitude, double duration)
    {
        _shakeAmplitude = (float)amplitude;
        _shakeDuration = (float)duration;
        _shakeTimer = (float)duration;
    }

    public void FadeIn(double duration, byte r, byte g, byte b, byte a) { }
    public void FadeOut(double duration, byte r, byte g, byte b, byte a) { }

    public void Flash(byte r, byte g, byte b, byte a, double duration)
    {
        _flashR = r; _flashG = g; _flashB = b; _flashA = a;
        _flashDuration = duration;
        _flashTimer = duration;
    }

    public void Tween(TdValue target, string property, double toValue, double duration)
    {
        if (target.Type == TdValueType.Object && target.ObjectValue.Fields.ContainsKey(property))
            target.ObjectValue.Fields[property] = TdValue.NumVal(toValue);
    }

    public void Trail(string spritePath, bool enabled) { }

    public bool GamepadHeld(string button)
    {
        var gp = MapGamepadButton(button);
        return gp.HasValue && Raylib.IsGamepadButtonDown(0, gp.Value);
    }

    public double GamepadAxis(string axis)
    {
        return axis switch
        {
            "left_x" => Raylib.GetGamepadAxisMovement(0, Raylib_cs.GamepadAxis.LeftX),
            "left_y" => Raylib.GetGamepadAxisMovement(0, Raylib_cs.GamepadAxis.LeftY),
            "right_x" => Raylib.GetGamepadAxisMovement(0, Raylib_cs.GamepadAxis.RightX),
            "right_y" => Raylib.GetGamepadAxisMovement(0, Raylib_cs.GamepadAxis.RightY),
            _ => 0
        };
    }

    public void SaveData(string path, TdValue data)
    {
        try { File.WriteAllText(path, data.ToString()); } catch { }
    }

    public TdValue LoadData(string path)
    {
        try { return TdValue.StrVal(File.ReadAllText(path)); } catch { return TdValue.NilVal(); }
    }

    public double Timer(double seconds) => seconds;
    public bool TimerElapsed(double timerId) => false;
    public void Animate(string spritePath, string animation, double dt) { }
    public void AddAnimation(string name, List<int> frames, double speed) { }
    public int GetFrame(string spritePath) => 0;
    public void SetFrame(string spritePath, int frame) { }
    public bool AnimationFinished(string spritePath) => false;

    public void CameraFollow(TdValue target, double smoothness)
    {
        _cameraFollowing = true;
        _followTarget = target;
        _followSmoothness = (float)smoothness;
    }

    public void CameraZoom(double zoom) => _cameraZoom = (float)zoom;
    public void CameraRotate(double degrees) => _cameraRotation = (float)degrees;

    public void CameraBounds(double x, double y, double w, double h)
    {
        _cameraBounds = new Rectangle((float)x, (float)y, (float)w, (float)h);
    }

    public TdValue WorldToScreen(double worldX, double worldY)
    {
        var worldPos = new Vector2((float)worldX, (float)worldY);
        var screenPos = Raylib.GetWorldToScreen2D(worldPos, new Camera2D
        {
            Target = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f),
            Offset = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f),
            Rotation = _cameraRotation,
            Zoom = _cameraZoom
        });
        return TdValue.Vec2Val(screenPos.X, screenPos.Y);
    }

    public void DrawHitbox(bool enabled) => _drawHitbox = enabled;
    public void ShowFps(bool enabled) => _showFps = enabled;
    public void Profile(string functionName) { }
    public void Breakpoint() { System.Diagnostics.Debugger.Break(); }
    public void StepFrame() { }

    public double Noise(double x, double y)
    {
        return (Math.Sin(x * 12.9898 + y * 78.233) * 43758.5453) % 1.0;
    }

    public TdValue Grid(int w, int h)
    {
        var grid = new List<TdValue>();
        for (int row = 0; row < h; row++)
        {
            var rowData = new List<TdValue>();
            for (int col = 0; col < w; col++)
                rowData.Add(TdValue.NumVal(0));
            grid.Add(TdValue.ArrVal(rowData));
        }
        return TdValue.ArrVal(grid);
    }

    public void FillRectMap(TdValue map, int x, int y, int w, int h, int tileId)
    {
        if (map.Type != TdValueType.Array) return;
        int startRow = Math.Max(0, y);
        int startCol = Math.Max(0, x);
        for (int row = startRow; row < y + h && row < map.ArrayValue.Count; row++)
        {
            var rowData = map.ArrayValue[row];
            if (rowData.Type != TdValueType.Array) continue;
            for (int col = startCol; col < x + w && col < rowData.ArrayValue.Count; col++)
                rowData.ArrayValue[col] = TdValue.NumVal(tileId);
        }
    }

    public TdValue FindPath(double startX, double startY, double endX, double endY)
    {
        var path = new List<TdValue>
        {
            TdValue.Vec2Val(startX, startY),
            TdValue.Vec2Val(endX, endY)
        };
        return TdValue.ArrVal(path);
    }

    public double PathLength(TdValue path)
    {
        if (path.Type != TdValueType.Array || path.ArrayValue.Count < 2) return 0;
        double total = 0;
        for (int i = 1; i < path.ArrayValue.Count; i++)
        {
            var a = path.ArrayValue[i - 1];
            var b = path.ArrayValue[i];
            if (a.Type == TdValueType.Vector2 && b.Type == TdValueType.Vector2)
            {
                double dx = a.VecX - b.VecX, dy = a.VecY - b.VecY;
                total += Math.Sqrt(dx * dx + dy * dy);
            }
        }
        return total;
    }

    public void Reload() { _running = false; }
    public void Record(string path) { }
    public void Playback(string path) { }
    public double TimeScale() => _timeScale;

    private static KeyboardKey? MapKey(string key)
    {
        return key.ToLower() switch
        {
            "up" => KeyboardKey.Up, "down" => KeyboardKey.Down,
            "left" => KeyboardKey.Left, "right" => KeyboardKey.Right,
            "space" => KeyboardKey.Space, "enter" => KeyboardKey.Enter,
            "escape" => KeyboardKey.Escape, "tab" => KeyboardKey.Tab,
            "shift" => KeyboardKey.LeftShift, "control" or "ctrl" => KeyboardKey.LeftControl,
            "a" => KeyboardKey.A, "b" => KeyboardKey.B, "c" => KeyboardKey.C,
            "d" => KeyboardKey.D, "e" => KeyboardKey.E, "f" => KeyboardKey.F,
            "g" => KeyboardKey.G, "h" => KeyboardKey.H, "i" => KeyboardKey.I,
            "j" => KeyboardKey.J, "k" => KeyboardKey.K, "l" => KeyboardKey.L,
            "m" => KeyboardKey.M, "n" => KeyboardKey.N, "o" => KeyboardKey.O,
            "p" => KeyboardKey.P, "q" => KeyboardKey.Q, "r" => KeyboardKey.R,
            "s" => KeyboardKey.S, "t" => KeyboardKey.T, "u" => KeyboardKey.U,
            "v" => KeyboardKey.V, "w" => KeyboardKey.W, "x" => KeyboardKey.X,
            "y" => KeyboardKey.Y, "z" => KeyboardKey.Z,
            "0" => KeyboardKey.Zero, "1" => KeyboardKey.One, "2" => KeyboardKey.Two,
            "3" => KeyboardKey.Three, "4" => KeyboardKey.Four, "5" => KeyboardKey.Five,
            "6" => KeyboardKey.Six, "7" => KeyboardKey.Seven, "8" => KeyboardKey.Eight,
            "9" => KeyboardKey.Nine,
            _ => null
        };
    }

    private static GamepadButton? MapGamepadButton(string button)
    {
        return button.ToUpper() switch
        {
            "A" => GamepadButton.RightFaceDown,
            "B" => GamepadButton.RightFaceRight,
            "X" => GamepadButton.RightFaceLeft,
            "Y" => GamepadButton.RightFaceUp,
            "START" => GamepadButton.MiddleRight,
            "SELECT" => GamepadButton.MiddleLeft,
            _ => null
        };
    }
}


