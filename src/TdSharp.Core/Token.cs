namespace TdSharp.Core;

public enum TokenType
{
    // Built-in functions (console & game)
    Say, Show, Clear, Rect, Circle, Line, Tilemap,
    Holding, Tap, MouseX, MouseY,
    Roll, Abs, Min, Max, Clamp, Dist, Smooth,
    GoTo, Remove, Spawn, Quit,

    // SFX
    PlaySound, PlayMusic, StopMusic, SetSoundVolume, SetMusicVolume, IsPlaying,

    // VFX
    Emit, Shake, FadeIn, FadeOut, Flash, Tween, Trail, Tint,

    // Extended
    GamepadHeld, GamepadAxis, After, SaveData, LoadData, Timer, TimerElapsed,

    // Animation
    Animate, AddAnimation, GetFrame, SetFrame, AnimationFinished,

    // Camera
    CameraFollow, CameraZoom, CameraRotate, CameraBounds, WorldToScreen,

    // Debug
    DrawHitbox, ShowFps, Profile, Inspect, Breakpoint, StepFrame,

    // Procedural
    Noise, Grid, FillRectMap, FindPath, PathLength,

    // Dev tools
    Reload, Record, Playback, TimeScale,

    // Human keywords
    Blueprint, Copies, SendBack, AsLongAs, RepeatH, StopH, SkipH,

    // Structural
    Function, End, If, Elif, Else, Then, While, For, In, To, Do, Match, Arrow,
    Class, Constructor, This,
    Var, Const, Local, Import, Return, Break, Continue,
    And, Or, Not, Nil, True, False,

    // Literals
    Number, String, HexColor, Identifier,

    // Operators
    Plus, Minus, Star, Slash, Percent, Caret,
    PlusEquals, MinusEquals, StarEquals, SlashEquals,
    EqualEqual, BangEqual, Greater, GreaterEqual, Less, LessEqual,
    Equal, Dot, DotDot, Comma, Colon, LParen, RParen, LBracket, RBracket, LBrace, RBrace, Hash,
    Newline, EOF
}

public class Token
{
    public TokenType Type { get; }
    public string Lexeme { get; }
    public object Literal { get; }
    public int Line { get; }

    public Token(TokenType type, string lexeme, object literal, int line)
    {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        Line = line;
    }

    public override string ToString() => $"{Type} '{Lexeme}'";
}
