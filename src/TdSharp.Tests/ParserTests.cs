using TdSharp.Core;
using Xunit;

namespace TdSharp.Tests;

public class ParserTests
{
    [Fact]
    public void Parses_VariableDeclaration()
    {
        var lexer = new Lexer("var x = 5");
        var parser = new Parser(lexer.Tokenize());
        var program = parser.Parse();
        Assert.Single(program);
        Assert.IsType<Stmt.Var>(program[0]);
        var varStmt = (Stmt.Var)program[0];
        Assert.Equal("x", varStmt.Name.Lexeme);
        Assert.NotNull(varStmt.Initializer);
    }

    [Fact]
    public void Parses_IfStatement()
    {
        var lexer = new Lexer("if true then\nsay(\"y\")\nend");
        var parser = new Parser(lexer.Tokenize());
        var program = parser.Parse();
        Assert.Single(program);
        Assert.IsType<Stmt.If>(program[0]);
    }

    [Fact]
    public void Parses_FunctionDeclaration()
    {
        var lexer = new Lexer("function add(a,b)\nreturn a+b\nend");
        var parser = new Parser(lexer.Tokenize());
        var program = parser.Parse();
        Assert.Single(program);
        Assert.IsType<Stmt.Function>(program[0]);
        var func = (Stmt.Function)program[0];
        Assert.Equal("add", func.Name.Lexeme);
        Assert.Equal(2, func.Parameters.Count);
    }

    [Fact]
    public void Parses_ForLoop()
    {
        var lexer = new Lexer("for i = 1 to 5 do\nsay(i)\nend");
        var parser = new Parser(lexer.Tokenize());
        var program = parser.Parse();
        Assert.Single(program);
        Assert.IsType<Stmt.For>(program[0]);
    }

    [Fact]
    public void Parses_WhileLoop()
    {
        var lexer = new Lexer("while x < 5 do\nx = x + 1\nend");
        var parser = new Parser(lexer.Tokenize());
        var program = parser.Parse();
        Assert.Single(program);
        Assert.IsType<Stmt.While>(program[0]);
    }

    [Fact]
    public void Parses_Class()
    {
        var lexer = new Lexer("class Enemy\nfunction constructor(x)\nthis.x=x\nend\nfunction move()\nend\nend");
        var parser = new Parser(lexer.Tokenize());
        var program = parser.Parse();
        Assert.Single(program);
        Assert.IsType<Stmt.Class>(program[0]);
        var cls = (Stmt.Class)program[0];
        Assert.Equal("Enemy", cls.Name.Lexeme);
        Assert.Single(cls.Methods);
        Assert.NotNull(cls.Constructor);
    }
}
