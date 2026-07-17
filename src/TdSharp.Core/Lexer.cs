using System.Globalization;
using System.Text;

namespace TdSharp.Core;

public class Lexer
{
    private readonly string _source;
    private readonly List<Token> _tokens = new();
    private int _start;
    private int _current;
    private int _line = 1;

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        {"say", TokenType.Say},
        {"show", TokenType.Show},
        {"clear", TokenType.Clear},
        {"rect", TokenType.Rect},
        {"circle", TokenType.Circle},
        {"line", TokenType.Line},
        {"tilemap", TokenType.Tilemap},
        {"holding", TokenType.Holding},
        {"tap", TokenType.Tap},
        {"mouse_x", TokenType.MouseX},
        {"mouse_y", TokenType.MouseY},
        {"roll", TokenType.Roll},
        {"abs", TokenType.Abs},
        {"min", TokenType.Min},
        {"max", TokenType.Max},
        {"clamp", TokenType.Clamp},
        {"dist", TokenType.Dist},
        {"smooth", TokenType.Smooth},
        {"go_to", TokenType.GoTo},
        {"remove", TokenType.Remove},
        {"spawn", TokenType.Spawn},
        {"quit", TokenType.Quit},
        {"play_sound", TokenType.PlaySound},
        {"play_music", TokenType.PlayMusic},
        {"stop_music", TokenType.StopMusic},
        {"set_sound_volume", TokenType.SetSoundVolume},
        {"set_music_volume", TokenType.SetMusicVolume},
        {"is_playing", TokenType.IsPlaying},
        {"emit", TokenType.Emit},
        {"shake", TokenType.Shake},
        {"fade_in", TokenType.FadeIn},
        {"fade_out", TokenType.FadeOut},
        {"flash", TokenType.Flash},
        {"tween", TokenType.Tween},
        {"trail", TokenType.Trail},
        {"tint", TokenType.Tint},
        {"gamepad_held", TokenType.GamepadHeld},
        {"gamepad_axis", TokenType.GamepadAxis},
        {"after", TokenType.After},
        {"save_data", TokenType.SaveData},
        {"load_data", TokenType.LoadData},
        {"timer", TokenType.Timer},
        {"timer_elapsed", TokenType.TimerElapsed},
        {"animate", TokenType.Animate},
        {"add_animation", TokenType.AddAnimation},
        {"get_frame", TokenType.GetFrame},
        {"set_frame", TokenType.SetFrame},
        {"animation_finished", TokenType.AnimationFinished},
        {"camera_follow", TokenType.CameraFollow},
        {"camera_zoom", TokenType.CameraZoom},
        {"camera_rotate", TokenType.CameraRotate},
        {"camera_bounds", TokenType.CameraBounds},
        {"world_to_screen", TokenType.WorldToScreen},
        {"draw_hitbox", TokenType.DrawHitbox},
        {"show_fps", TokenType.ShowFps},
        {"profile", TokenType.Profile},
        {"inspect", TokenType.Inspect},
        {"breakpoint", TokenType.Breakpoint},
        {"step_frame", TokenType.StepFrame},
        {"noise", TokenType.Noise},
        {"grid", TokenType.Grid},
        {"fill_rect_map", TokenType.FillRectMap},
        {"find_path", TokenType.FindPath},
        {"path_length", TokenType.PathLength},
        {"reload", TokenType.Reload},
        {"record", TokenType.Record},
        {"playback", TokenType.Playback},
        {"time_scale", TokenType.TimeScale},
        {"blueprint", TokenType.Blueprint},
        {"copies", TokenType.Copies},
        {"send_back", TokenType.SendBack},
        {"as_long_as", TokenType.AsLongAs},
        {"repeat", TokenType.RepeatH},
        {"stop", TokenType.StopH},
        {"skip", TokenType.SkipH},
        {"function", TokenType.Function},
        {"end", TokenType.End},
        {"if", TokenType.If},
        {"elif", TokenType.Elif},
        {"else", TokenType.Else},
        {"while", TokenType.While},
        {"for", TokenType.For},
        {"in", TokenType.In},
        {"then", TokenType.Then},
        {"to", TokenType.To},
        {"do", TokenType.Do},
        {"match", TokenType.Match},
        {"class", TokenType.Class},
        {"constructor", TokenType.Constructor},
        {"this", TokenType.This},
        {"var", TokenType.Var},
        {"const", TokenType.Const},
        {"local", TokenType.Local},
        {"import", TokenType.Import},
        {"return", TokenType.Return},
        {"break", TokenType.Break},
        {"continue", TokenType.Continue},
        {"and", TokenType.And},
        {"or", TokenType.Or},
        {"not", TokenType.Not},
        {"nil", TokenType.Nil},
        {"true", TokenType.True},
        {"false", TokenType.False},
    };

    public Lexer(string source)
    {
        _source = source;
    }

    public List<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }
        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(': AddToken(TokenType.LParen); break;
            case ')': AddToken(TokenType.RParen); break;
            case '[': AddToken(TokenType.LBracket); break;
            case ']': AddToken(TokenType.RBracket); break;
            case '{': AddToken(TokenType.LBrace); break;
            case '}': AddToken(TokenType.RBrace); break;
            case ',': AddToken(TokenType.Comma); break;
            case ':': AddToken(TokenType.Colon); break;

            case '-':
                if (Match('>')) AddToken(TokenType.Arrow);
                else if (Match('=')) AddToken(TokenType.MinusEquals);
                else AddToken(TokenType.Minus);
                break;

            case '+':
                AddToken(Match('=') ? TokenType.PlusEquals : TokenType.Plus);
                break;

            case '*':
                AddToken(Match('=') ? TokenType.StarEquals : TokenType.Star);
                break;

            case '/':
                if (Match('/'))
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                else if (Match('*'))
                    SkipBlockComment();
                else if (Match('='))
                    AddToken(TokenType.SlashEquals);
                else
                    AddToken(TokenType.Slash);
                break;

            case '%': AddToken(TokenType.Percent); break;
            case '^': AddToken(TokenType.Caret); break;

            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Not);
                break;

            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;

            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;

            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;

            case '.':
                AddToken(Match('.') ? TokenType.DotDot : TokenType.Dot);
                break;

            case '#':
                TryHexOrHash();
                break;

            case '"':
                ScanString();
                break;

            case '\n':
                AddToken(TokenType.Newline);
                break;

            case ' ':
            case '\r':
            case '\t':
                break;

            default:
                if (char.IsDigit(c))
                    ScanNumber();
                else if (char.IsLetter(c) || c == '_')
                    ScanIdentifier();
                else
                    throw new RuntimeException($"Unexpected character '{c}'.", _line);
                break;
        }
    }

    private void TryHexOrHash()
    {
        if (Peek() == '\0') { AddToken(TokenType.Hash); return; }
        int saved = _current;
        int hexDigits = 0;
        while (saved + hexDigits < _source.Length && IsHexDigit(_source[saved + hexDigits]))
            hexDigits++;
        // Valid hex colors: exactly 3 or 6 hex digits, then a non-hex-or-identifier char
        if ((hexDigits == 3 || hexDigits == 6) &&
            (saved + hexDigits >= _source.Length || !char.IsLetterOrDigit(_source[saved + hexDigits])))
        {
            // It's a hex color
            for (int i = 0; i < hexDigits; i++) Advance();
            string hex = _source.Substring(_start + 1, hexDigits);
            int value = int.Parse(hex, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
            byte r, g, b, a = 255;
            if (hexDigits == 6)
            {
                r = (byte)((value >> 16) & 0xFF);
                g = (byte)((value >> 8) & 0xFF);
                b = (byte)(value & 0xFF);
            }
            else
            {
                r = (byte)(((value >> 8) & 0xF) * 17);
                g = (byte)(((value >> 4) & 0xF) * 17);
                b = (byte)((value & 0xF) * 17);
            }
            AddToken(TokenType.HexColor, TdValue.ColorVal(r, g, b, a));
        }
        else
        {
            AddToken(TokenType.Hash);
        }
    }

    private void ScanHexColor()
    {
        while (IsHexDigit(Peek())) Advance();
        string hex = _source.Substring(_start + 1, _current - _start - 1);
        int value = int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte r, g, b, a = 255;
        if (hex.Length == 6)
        {
            r = (byte)((value >> 16) & 0xFF);
            g = (byte)((value >> 8) & 0xFF);
            b = (byte)(value & 0xFF);
        }
        else if (hex.Length == 3)
        {
            r = (byte)(((value >> 8) & 0xF) * 17);
            g = (byte)(((value >> 4) & 0xF) * 17);
            b = (byte)((value & 0xF) * 17);
        }
        else
        {
            throw new RuntimeException($"Invalid hex color '#{hex}'.", _line);
        }
        AddToken(TokenType.HexColor, TdValue.ColorVal(r, g, b, a));
    }

    private void ScanString()
    {
        var sb = new StringBuilder();
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\\')
            {
                Advance();
                if (IsAtEnd()) break;
                switch (Peek())
                {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case '\\': sb.Append('\\'); break;
                    case '"': sb.Append('"'); break;
                    default: sb.Append(Peek()); break;
                }
                Advance();
            }
            else
            {
                sb.Append(Advance());
            }
        }
        if (IsAtEnd())
            throw new RuntimeException("Unterminated string.", _line);
        Advance(); // closing "
        AddToken(TokenType.String, TdValue.StrVal(sb.ToString()));
    }

    private void ScanNumber()
    {
        while (char.IsDigit(Peek())) Advance();
        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            Advance();
            while (char.IsDigit(Peek())) Advance();
        }
        string numStr = _source.Substring(_start, _current - _start);
        if (!double.TryParse(numStr, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            throw new RuntimeException($"Invalid number '{numStr}'.", _line);
        AddToken(TokenType.Number, TdValue.NumVal(value));
    }

    private void ScanIdentifier()
    {
        while (char.IsLetterOrDigit(Peek()) || Peek() == '_') Advance();
        string text = _source.Substring(_start, _current - _start);
        TokenType type = Keywords.TryGetValue(text, out var tt) ? tt : TokenType.Identifier;
        AddToken(type);
    }

    private void SkipBlockComment()
    {
        int depth = 1;
        while (depth > 0 && !IsAtEnd())
        {
            if (Peek() == '/' && PeekNext() == '*') { Advance(); Advance(); depth++; }
            else if (Peek() == '*' && PeekNext() == '/') { Advance(); Advance(); depth--; }
            else { if (Peek() == '\n') _line++; Advance(); }
        }
        if (depth > 0)
            throw new RuntimeException("Unterminated block comment.", _line);
    }

    private char Advance() => _source[_current++];
    private char Peek() => IsAtEnd() ? '\0' : _source[_current];
    private char PeekNext() => _current + 1 >= _source.Length ? '\0' : _source[_current + 1];
    private bool IsAtEnd() => _current >= _source.Length;
    private bool Match(char expected)
    {
        if (IsAtEnd() || _source[_current] != expected) return false;
        _current++;
        return true;
    }

    private bool IsHexDigit(char c) =>
        char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

    private void AddToken(TokenType type, object literal = null)
    {
        string text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, literal, _line));
    }
}
