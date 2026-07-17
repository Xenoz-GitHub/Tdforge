using TdSharp.Core;
using Xunit;

namespace TdSharp.Tests;

public class LexerTests
{
    [Fact]
    public void Tokenizes_Say()
    {
        var lexer = new Lexer("say(\"hello\")");
        var tokens = lexer.Tokenize();
        Assert.Equal(TokenType.Say, tokens[0].Type);
        Assert.Equal(TokenType.LParen, tokens[1].Type);
        Assert.Equal(TokenType.String, tokens[2].Type);
        Assert.Equal("hello", (string)((TdValue)tokens[2].Literal).StrValue);
        Assert.Equal(TokenType.RParen, tokens[3].Type);
        Assert.Equal(TokenType.EOF, tokens[^1].Type);
    }

    [Fact]
    public void Tokenizes_Numbers()
    {
        var lexer = new Lexer("42 3.14");
        var tokens = lexer.Tokenize();
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal(42.0, ((TdValue)tokens[0].Literal).NumValue);
        Assert.Equal(TokenType.Number, tokens[1].Type);
        Assert.Equal(3.14, ((TdValue)tokens[1].Literal).NumValue);
    }

    [Fact]
    public void Tokenizes_HexColor()
    {
        var lexer = new Lexer("#FF5733");
        var tokens = lexer.Tokenize();
        Assert.Equal(TokenType.HexColor, tokens[0].Type);
        var color = (TdValue)tokens[0].Literal;
        Assert.Equal(255, color.R);
        Assert.Equal(87, color.G);
        Assert.Equal(51, color.B);
    }

    [Fact]
    public void Tokenizes_Hash_ForLength()
    {
        var lexer = new Lexer("#items");
        var tokens = lexer.Tokenize();
        Assert.Equal(TokenType.Hash, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal("items", tokens[1].Lexeme);
    }

    [Fact]
    public void Tokenizes_Operators()
    {
        var lexer = new Lexer("+ - * / % ^ ..");
        var tokens = lexer.Tokenize();
        Assert.Equal(TokenType.Plus, tokens[0].Type);
        Assert.Equal(TokenType.Minus, tokens[1].Type);
        Assert.Equal(TokenType.Star, tokens[2].Type);
        Assert.Equal(TokenType.Slash, tokens[3].Type);
        Assert.Equal(TokenType.Percent, tokens[4].Type);
        Assert.Equal(TokenType.Caret, tokens[5].Type);
        Assert.Equal(TokenType.DotDot, tokens[6].Type);
    }

    [Fact]
    public void Tokenizes_Comparison()
    {
        var lexer = new Lexer("== != > < >= <=");
        var tokens = lexer.Tokenize();
        Assert.Equal(TokenType.EqualEqual, tokens[0].Type);
        Assert.Equal(TokenType.BangEqual, tokens[1].Type);
        Assert.Equal(TokenType.Greater, tokens[2].Type);
        Assert.Equal(TokenType.Less, tokens[3].Type);
        Assert.Equal(TokenType.GreaterEqual, tokens[4].Type);
        Assert.Equal(TokenType.LessEqual, tokens[5].Type);
    }

    [Fact]
    public void Tokenizes_Keywords()
    {
        var lexer = new Lexer("if then else end while do for function return");
        var tokens = lexer.Tokenize();
        Assert.Equal(TokenType.If, tokens[0].Type);
        Assert.Equal(TokenType.Then, tokens[1].Type);
        Assert.Equal(TokenType.Else, tokens[2].Type);
        Assert.Equal(TokenType.End, tokens[3].Type);
        Assert.Equal(TokenType.While, tokens[4].Type);
        Assert.Equal(TokenType.Do, tokens[5].Type);
        Assert.Equal(TokenType.For, tokens[6].Type);
        Assert.Equal(TokenType.Function, tokens[7].Type);
        Assert.Equal(TokenType.Return, tokens[8].Type);
    }

    [Fact]
    public void SkipsComments()
    {
        var lexer = new Lexer("// comment\n42");
        var tokens = lexer.Tokenize();
        Assert.Equal(TokenType.Newline, tokens[0].Type);
        Assert.Equal(TokenType.Number, tokens[1].Type);
    }

    [Fact]
    public void SkipsBlockComments()
    {
        var lexer = new Lexer("/* block\ncomment */42");
        var tokens = lexer.Tokenize();
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal(42.0, ((TdValue)tokens[0].Literal).NumValue);
    }
}
