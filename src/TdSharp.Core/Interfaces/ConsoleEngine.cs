namespace TdSharp.Core;

public class ConsoleEngine : IGameEngine
{
    public bool IsActive => false;
    public void Clear(double r, double g, double b, double a) { }
    public void ShowSprite(string path, double x, double y, double rot, double scaleX, double scaleY, TdValue tint) { }
    public void ShowText(string text, double x, double y, double size, double r, double g, double b, double a) { }
    public void DrawRect(double x, double y, double w, double h, double r, double g, double b, double a) { }
    public void DrawCircle(double x, double y, double radius, double r, double g, double b, double a) { }
    public void DrawLine(double x1, double y1, double x2, double y2, double r, double g, double b, double a) { }
    public void DrawTilemap(TdValue tilemapData, double camX, double camY) { }
    public bool Holding(string key) => false;
    public bool MouseTap(string button) => false;
    public double MouseX() => 0;
    public double MouseY() => 0;
    public void PlaySound(string path) { }
    public void PlayMusic(string path) { }
    public void StopMusic() { }
    public void SetSoundVolume(double vol) { }
    public void SetMusicVolume(double vol) { }
    public bool IsPlaying(string path) => false;
    public void Emit(string particleType, double x, double y, int count) { }
    public void Shake(double amplitude, double duration) { }
    public void FadeIn(double duration, byte r, byte g, byte b, byte a) { }
    public void FadeOut(double duration, byte r, byte g, byte b, byte a) { }
    public void Flash(byte r, byte g, byte b, byte a, double duration) { }
    public void Tween(TdValue target, string property, double toValue, double duration) { }
    public void Trail(string spritePath, bool enabled) { }
    public bool GamepadHeld(string button) => false;
    public double GamepadAxis(string axis) => 0;
    public void SaveData(string path, TdValue data) { }
    public TdValue LoadData(string path) => TdValue.NilVal();
    public double Timer(double seconds) => 0;
    public bool TimerElapsed(double timerId) => false;
    public void Animate(string spritePath, string animation, double dt) { }
    public void AddAnimation(string name, List<int> frames, double speed) { }
    public int GetFrame(string spritePath) => 0;
    public void SetFrame(string spritePath, int frame) { }
    public bool AnimationFinished(string spritePath) => false;
    public void CameraFollow(TdValue target, double smoothness) { }
    public void CameraZoom(double zoom) { }
    public void CameraRotate(double degrees) { }
    public void CameraBounds(double x, double y, double w, double h) { }
    public TdValue WorldToScreen(double worldX, double worldY) => TdValue.Vec2Val(worldX, worldY);
    public void DrawHitbox(bool enabled) { }
    public void ShowFps(bool enabled) { }
    public void Profile(string functionName) { }
    public void Breakpoint() { }
    public void StepFrame() { }
    public double Noise(double x, double y) => 0;
    public TdValue Grid(int w, int h) => TdValue.ArrVal(new List<TdValue>());
    public void FillRectMap(TdValue map, int x, int y, int w, int h, int tileId) { }
    public TdValue FindPath(double startX, double startY, double endX, double endY) => TdValue.NilVal();
    public double PathLength(TdValue path) => 0;
    public void Reload() { }
    public void Record(string path) { }
    public void Playback(string path) { }
    public double TimeScale() => 1.0;
}
