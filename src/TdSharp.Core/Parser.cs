namespace TdSharp.Core;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _current;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();
        while (!IsAtEnd())
        {
            Stmt stmt = Declaration();
            if (stmt != null)
                statements.Add(stmt);
            // consume newlines between statements
            while (Match(TokenType.Newline)) { }
        }
        return statements;
    }

    private Stmt Declaration()
    {
        try
        {
            if (Match(TokenType.Var) || Match(TokenType.Local))
                return VarDeclaration(MatchPrevType == TokenType.Local);
            if (Match(TokenType.Const))
                return ConstDeclaration();
            if (Match(TokenType.Function))
                return FunctionDeclaration("function");
            if (Match(TokenType.Class) || Match(TokenType.Blueprint))
                return ClassDeclaration();
            if (Match(TokenType.AsLongAs))
                return WhileStatement();
            if (Match(TokenType.Return) || Match(TokenType.SendBack))
                return ReturnStatement();
            if (Match(TokenType.Import))
                return ImportStatement();
            if (Match(TokenType.Break) || Match(TokenType.StopH))
                return BreakStatement();
            if (Match(TokenType.Continue) || Match(TokenType.SkipH))
                return ContinueStatement();

            return Statement();
        }
        catch (RuntimeException)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt VarDeclaration(bool isLocal)
    {
        Token name = Consume(TokenType.Identifier, "Expected variable name.");

        Expr initializer = null;
        if (Match(TokenType.Equal))
            initializer = Expression();

        return new Stmt.Var(name, initializer, isLocal);
    }

    private Stmt ConstDeclaration()
    {
        Token name = Consume(TokenType.Identifier, "Expected constant name.");
        Consume(TokenType.Equal, "Expected '=' after constant name.");
        Expr initializer = Expression();
        return new Stmt.Const(name, initializer);
    }

    private Stmt Statement()
    {
        if (Match(TokenType.If))
            return IfStatement();
        if (Match(TokenType.While))
            return WhileStatement();
        if (Match(TokenType.For) || Match(TokenType.RepeatH))
            return ForStatement();
        if (Match(TokenType.Match))
            return MatchStatement();
        if (Match(TokenType.LBrace))
            return BlockStatement();

        return ExpressionStatement();
    }

    private Stmt IfStatement()
    {
        Expr condition = Expression();
        Consume(TokenType.Then, "Expected 'then' after if condition.");

        var thenBranch = new List<Stmt>();
        while (!Check(TokenType.Elif) && !Check(TokenType.Else) && !Check(TokenType.End) && !IsAtEnd())
        {
            Stmt s = Declaration();
            if (s != null) thenBranch.Add(s);
        }

        var elifBranches = new List<Stmt>();
        while (Match(TokenType.Elif))
        {
            Expr elifCondition = Expression();
            Consume(TokenType.Then, "Expected 'then' after elif condition.");
            var elifBody = new List<Stmt>();
            while (!Check(TokenType.Elif) && !Check(TokenType.Else) && !Check(TokenType.End) && !IsAtEnd())
            {
                Stmt s = Declaration();
                if (s != null) elifBody.Add(s);
            }
            elifBranches.Add(new Stmt.If(elifCondition, elifBody, new List<Stmt>(), new List<Stmt>()));
        }

        var elseBranch = new List<Stmt>();
        if (Match(TokenType.Else))
        {
            while (!Check(TokenType.End) && !IsAtEnd())
            {
                Stmt s = Declaration();
                if (s != null) elseBranch.Add(s);
            }
        }

        Consume(TokenType.End, "Expected 'end' after if block.");
        return new Stmt.If(condition, thenBranch, elifBranches, elseBranch);
    }

    private Stmt WhileStatement()
    {
        Expr condition = Expression();
        Consume(TokenType.Do, "Expected 'do' after while condition.");

        var body = new List<Stmt>();
        while (!Check(TokenType.End) && !IsAtEnd())
        {
            Stmt s = Declaration();
            if (s != null) body.Add(s);
        }
        Consume(TokenType.End, "Expected 'end' after while block.");
        return new Stmt.While(condition, body);
    }

    private Stmt ForStatement()
    {
        Token variable = Consume(TokenType.Identifier, "Expected loop variable name.");

        // Check for "for each" pattern: "for item in collection do"
        if (Match(TokenType.In))
        {
            Expr collection = Expression();
            Consume(TokenType.Do, "Expected 'do' after for-each collection.");

            var body = new List<Stmt>();
            while (!Check(TokenType.End) && !IsAtEnd())
            {
                Stmt s = Declaration();
                if (s != null) body.Add(s);
            }
            Consume(TokenType.End, "Expected 'end' after for-each block.");
            return new Stmt.ForEach(variable, collection, body);
        }

        // Numeric for: "for i = 1 to 10 do"
        Expr start;
        if (Match(TokenType.Equal))
            start = Expression();
        else
            start = new Expr.Literal(TdValue.NumVal(0));

        Token toToken = ConsumeMatching(TokenType.To, "to");
        if (toToken == null)
            throw new RuntimeException("Expected 'to' in for loop range.", Peek().Line);

        Expr end = Expression();
        Consume(TokenType.Do, "Expected 'do' after for loop range.");

        var bodyList = new List<Stmt>();
        while (!Check(TokenType.End) && !IsAtEnd())
        {
            Stmt s = Declaration();
            if (s != null) bodyList.Add(s);
        }
        Consume(TokenType.End, "Expected 'end' after for block.");
        return new Stmt.For(variable, start, end, bodyList);
    }

    private Stmt MatchStatement()
    {
        Expr value = Expression();
        Consume(TokenType.Newline, "Expected newline after match value.");

        var cases = new List<(Expr Pattern, List<Stmt> Body)>();
        var elseBody = new List<Stmt>();

        while (!Check(TokenType.End) && !IsAtEnd())
        {
            if (Match(TokenType.Else))
            {
                Consume(TokenType.Arrow, "Expected '->' after else in match.");
                elseBody.Add(new Stmt.Expression(ParseSingleExpression()));
                // Rest of else on same line or block
                if (Match(TokenType.Newline))
                {
                    while (!Check(TokenType.End) && !IsAtEnd())
                    {
                        Stmt s = Declaration();
                        if (s != null) elseBody.Add(s);
                    }
                }
                break;
            }

            Expr pattern = Expression();
            Consume(TokenType.Arrow, "Expected '->' in match case.");
            var body = new List<Stmt>();
            body.Add(new Stmt.Expression(ParseSingleExpression()));
            // multiple statements on following lines
            if (Match(TokenType.Newline))
            {
                while (!Check(TokenType.End) && !Check(TokenType.Else) && !IsAtEnd())
                {
                    Stmt s = Declaration();
                    if (s != null) body.Add(s);
                }
            }
            cases.Add((pattern, body));
        }

        Consume(TokenType.End, "Expected 'end' after match block.");
        return new Stmt.Match(value, cases, elseBody);
    }

    private Stmt FunctionDeclaration(string kind, bool allowConstructor = false)
    {
        Token name;
        if (allowConstructor && Match(TokenType.Constructor))
            name = new Token(TokenType.Identifier, "constructor", null, Previous().Line);
        else
            name = Consume(TokenType.Identifier, $"Expected {kind} name.");
        Consume(TokenType.LParen, "Expected '(' after function name.");
        var parameters = new List<Token>();
        if (!Check(TokenType.RParen))
        {
            do
            {
                if (parameters.Count >= 255)
                    throw new RuntimeException("Can't have more than 255 parameters.", Peek().Line);
                parameters.Add(Consume(TokenType.Identifier, "Expected parameter name."));
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RParen, "Expected ')' after parameters.");

        // Optional newline before body
        if (Match(TokenType.Newline)) { }

        var body = new List<Stmt>();
        while (!Check(TokenType.End) && !IsAtEnd())
        {
            Stmt s = Declaration();
            if (s != null) body.Add(s);
        }
        Consume(TokenType.End, $"Expected 'end' after {kind} body.");
        return new Stmt.Function(name, parameters, body);
    }

    private Stmt ClassDeclaration()
    {
        Token name = Consume(TokenType.Identifier, "Expected class name.");

        string inheritsFrom = null;
        // Optional inheritance: "class Enemy copies BaseEnemy"
        // Or: "class Enemy copies BaseEnemy"
        // Actually the spec says: "copies" keyword
        if (Match(TokenType.Copies))
        {
            Token parent = Consume(TokenType.Identifier, "Expected parent class name.");
            inheritsFrom = parent.Lexeme;
        }

        // Consume newline after class header
        if (Match(TokenType.Newline)) { }

        List<Stmt.Function> methods = new();
        Stmt.Function constructor = null;

        while (!Check(TokenType.End) && !IsAtEnd())
        {
            // Could be function or constructor
            if (Match(TokenType.Function))
            {
                if (Match(TokenType.Constructor))
                {
                    // constructor() special
                    Consume(TokenType.LParen, "Expected '(' after constructor.");
                    var cParams = new List<Token>();
                    if (!Check(TokenType.RParen))
                    {
                        do
                        {
                            cParams.Add(Consume(TokenType.Identifier, "Expected parameter name."));
                        } while (Match(TokenType.Comma));
                    }
                    Consume(TokenType.RParen, "Expected ')' after parameters.");
                    if (Match(TokenType.Newline)) { }

                    var cBody = new List<Stmt>();
                    while (!Check(TokenType.End) && !IsAtEnd())
                    {
                        Stmt s = Declaration();
                        if (s != null) cBody.Add(s);
                    }
                    Consume(TokenType.End, "Expected 'end' after constructor body.");
                    constructor = new Stmt.Function(
                        new Token(TokenType.Constructor, "constructor", null, name.Line),
                        cParams, cBody);
                }
                else
                {
                    // Regular method (or constructor if named that way)
                    var methodDecl = FunctionDeclaration("function", true);
                    if (methodDecl is Stmt.Function f)
                        methods.Add(f);
                }
            }
            else
            {
                // Error - skip to end
                Advance();
            }
        }

        Consume(TokenType.End, "Expected 'end' after class body.");
        return new Stmt.Class(name, inheritsFrom, methods, constructor);
    }

    private Stmt ReturnStatement()
    {
        Token keyword = Previous();
        Expr value = null;
        if (!Check(TokenType.Newline) && !Check(TokenType.End) && !Check(TokenType.Elif) &&
            !Check(TokenType.Else) && !Check(TokenType.EOF))
            value = Expression();
        return new Stmt.Return(keyword, value);
    }

    private Stmt BreakStatement()
    {
        Token keyword = Previous();
        return new Stmt.Break(keyword);
    }

    private Stmt ContinueStatement()
    {
        Token keyword = Previous();
        return new Stmt.Continue(keyword);
    }

    private Stmt ImportStatement()
    {
        Token keyword = Previous();
        Expr path = Expression();
        return new Stmt.Import(keyword, path);
    }

    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();

        // Check for assignment by identifier
        if (expr is Expr.Variable varExpr && Match(TokenType.Equal))
        {
            Expr value = Expression();
            // Check for compound assignment
            return new Stmt.Expression(new Expr.Assign(varExpr.Name, value));
        }

        // Check for compound assignment operators
        if (expr is Expr.Variable varExpr2)
        {
            if (Match(TokenType.PlusEquals) || Match(TokenType.MinusEquals) ||
                Match(TokenType.StarEquals) || Match(TokenType.SlashEquals))
            {
                TokenType opType = MatchPrevType;
                Expr value = Expression();
                // Transform: x += y  =>  x = x + y
                Token opToken = opType switch
                {
                    TokenType.PlusEquals => new Token(TokenType.Plus, "+", null, varExpr2.Name.Line),
                    TokenType.MinusEquals => new Token(TokenType.Minus, "-", null, varExpr2.Name.Line),
                    TokenType.StarEquals => new Token(TokenType.Star, "*", null, varExpr2.Name.Line),
                    TokenType.SlashEquals => new Token(TokenType.Slash, "/", null, varExpr2.Name.Line),
                    _ => throw new RuntimeException("Invalid assignment operator.", varExpr2.Name.Line)
                };
                Expr compound = new Expr.Binary(varExpr2, opToken, value);
                return new Stmt.Expression(new Expr.Assign(varExpr2.Name, compound));
            }
        }

        // Handle "set" on object property: obj.prop = value
        // This is handled by the parser naturally since = is right-associative
        // But the issue is assignment in expression context. Let me handle it differently.
        // Actually, for Td#, assignment is a statement, not an expression.
        // So we handle it here.

        return new Stmt.Expression(expr);
    }

    private Stmt BlockStatement()
    {
        var statements = new List<Stmt>();
        while (!Check(TokenType.RBrace) && !IsAtEnd())
        {
            Stmt s = Declaration();
            if (s != null) statements.Add(s);
        }
        Consume(TokenType.RBrace, "Expected '}' after block.");
        return new Stmt.Block(statements);
    }

    // Expression parsing

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        Expr expr = LogicalOr();

        if (Match(TokenType.Equal))
        {
            Token equals = Previous();
            Expr value = Assignment();

            if (expr is Expr.Variable varExpr)
                return new Expr.Assign(varExpr.Name, value);
            else if (expr is Expr.Get getExpr)
                return new Expr.Set(getExpr.Object, getExpr.Name, value);
            else if (expr is Expr.Index indexExpr)
            {
                // array[index] = value => transform to Set with special handling
                // For now, handle this in the interpreter
                return new Expr.Set(indexExpr.Object,
                    new Token(TokenType.Identifier, "__index", null, equals.Line), value);
            }

            throw new RuntimeException("Invalid assignment target.", equals.Line);
        }

        return expr;
    }

    private Expr LogicalOr()
    {
        Expr expr = LogicalAnd();
        while (Match(TokenType.Or))
        {
            Token op = Previous();
            Expr right = LogicalAnd();
            expr = new Expr.Logical(expr, op, right);
        }
        return expr;
    }

    private Expr LogicalAnd()
    {
        Expr expr = Comparison();
        while (Match(TokenType.And))
        {
            Token op = Previous();
            Expr right = Comparison();
            expr = new Expr.Logical(expr, op, right);
        }
        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Addition();
        while (Match(TokenType.EqualEqual) || Match(TokenType.BangEqual) ||
               Match(TokenType.Greater) || Match(TokenType.GreaterEqual) ||
               Match(TokenType.Less) || Match(TokenType.LessEqual))
        {
            Token op = Previous();
            Expr right = Addition();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    private Expr Addition()
    {
        Expr expr = Multiplication();
        while (Match(TokenType.Plus) || Match(TokenType.Minus) || Match(TokenType.DotDot))
        {
            Token op = Previous();
            Expr right = Multiplication();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    private Expr Multiplication()
    {
        Expr expr = Power();
        while (Match(TokenType.Star) || Match(TokenType.Slash) || Match(TokenType.Percent))
        {
            Token op = Previous();
            Expr right = Power();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    private Expr Power()
    {
        Expr expr = Unary();
        while (Match(TokenType.Caret))
        {
            Token op = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    private Expr Unary()
    {
            if (Match(TokenType.Minus) || Match(TokenType.Not) || Match(TokenType.Hash))
            {
                Token op = Previous();
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }
        return Call();
    }

    private Expr Call()
    {
        Expr expr = Primary();

        while (true)
        {
            if (Match(TokenType.LParen))
            {
                var args = new List<Expr>();
                if (!Check(TokenType.RParen))
                {
                    do
                    {
                        args.Add(Expression());
                    } while (Match(TokenType.Comma));
                }
                Token paren = Consume(TokenType.RParen, "Expected ')' after arguments.");
                expr = new Expr.Call(expr, paren, args);
            }
            else if (Match(TokenType.Dot))
            {
                Token name = Consume(TokenType.Identifier, "Expected property name after '.'.");
                expr = new Expr.Get(expr, name);
            }
            else if (Match(TokenType.LBracket))
            {
                Expr index = Expression();
                Consume(TokenType.RBracket, "Expected ']' after index.");
                expr = new Expr.Index(expr, index);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr Primary()
    {
        if (Match(TokenType.False)) return new Expr.Literal(TdValue.BoolVal(false));
        if (Match(TokenType.True)) return new Expr.Literal(TdValue.BoolVal(true));
        if (Match(TokenType.Nil)) return new Expr.Literal(TdValue.NilVal());

        if (Match(TokenType.Number))
            return new Expr.Literal((TdValue)Previous().Literal);

        if (Match(TokenType.String))
            return new Expr.Literal((TdValue)Previous().Literal);

        if (Match(TokenType.HexColor))
            return new Expr.Literal((TdValue)Previous().Literal);

        if (Match(TokenType.Identifier))
            return new Expr.Variable(Previous());

        if (Match(TokenType.This))
            return new Expr.Variable(Previous());

        // Built-in function keywords are callable as identifiers
        var callableTypes = new HashSet<TokenType>
        {
            TokenType.Say, TokenType.Show, TokenType.Clear, TokenType.Rect,
            TokenType.Circle, TokenType.Line, TokenType.Tilemap,
            TokenType.Holding, TokenType.Tap, TokenType.MouseX, TokenType.MouseY,
            TokenType.Roll, TokenType.Abs, TokenType.Min, TokenType.Max,
            TokenType.Clamp, TokenType.Dist, TokenType.Smooth,
            TokenType.GoTo, TokenType.Remove, TokenType.Spawn, TokenType.Quit,
            TokenType.PlaySound, TokenType.PlayMusic, TokenType.StopMusic,
            TokenType.SetSoundVolume, TokenType.SetMusicVolume, TokenType.IsPlaying,
            TokenType.Emit, TokenType.Shake, TokenType.FadeIn, TokenType.FadeOut,
            TokenType.Flash, TokenType.Tween, TokenType.Trail, TokenType.Tint,
            TokenType.GamepadHeld, TokenType.GamepadAxis, TokenType.After,
            TokenType.SaveData, TokenType.LoadData, TokenType.Timer, TokenType.TimerElapsed,
            TokenType.Animate, TokenType.AddAnimation, TokenType.GetFrame, TokenType.SetFrame,
            TokenType.AnimationFinished,
            TokenType.CameraFollow, TokenType.CameraZoom, TokenType.CameraRotate,
            TokenType.CameraBounds, TokenType.WorldToScreen,
            TokenType.DrawHitbox, TokenType.ShowFps, TokenType.Profile,
            TokenType.Inspect, TokenType.Breakpoint, TokenType.StepFrame,
            TokenType.Noise, TokenType.Grid, TokenType.FillRectMap,
            TokenType.FindPath, TokenType.PathLength,
            TokenType.Reload, TokenType.Record, TokenType.Playback, TokenType.TimeScale
        };
        if (callableTypes.Contains(Peek().Type))
        {
            Token t = Advance();
            return new Expr.Variable(t);
        }

        if (Match(TokenType.LParen))
        {
            Expr expr = Expression();
            Consume(TokenType.RParen, "Expected ')' after expression.");
            return new Expr.Grouping(expr);
        }

        if (Match(TokenType.LBracket))
        {
            var elements = new List<Expr>();
            if (!Check(TokenType.RBracket))
            {
                do
                {
                    elements.Add(Expression());
                } while (Match(TokenType.Comma));
            }
            Consume(TokenType.RBracket, "Expected ']' after array literal.");
            return new Expr.ArrayExpr(elements);
        }

        if (Match(TokenType.LBrace))
        {
            var entries = new List<(Token Key, Expr Value)>();
            if (!Check(TokenType.RBrace))
            {
                do
                {
                    Token key = Consume(TokenType.Identifier, "Expected key in map literal.");
                    Consume(TokenType.Equal, "Expected '=' after map key.");
                    Expr value = Expression();
                    entries.Add((key, value));
                } while (Match(TokenType.Comma));
            }
            Consume(TokenType.RBrace, "Expected '}' after map literal.");
            return new Expr.MapExpr(entries);
        }

        // Handle Vector2, Rect constructors
        // These are handled by the Call expression already
        // e.g., Vector2(10, 20) - parsed as Call(Variable("Vector2"), [10, 20])

        // Handle concatenation with ..
        // This is actually a binary operator, but let me handle it as part of addition
        // .. has same precedence as +

        throw new RuntimeException($"Unexpected token '{Peek().Lexeme}'.", Peek().Line);
    }

    // Helper: parse a single expression (for match cases etc.)
    private Expr ParseSingleExpression()
    {
        return Expression();
    }

    // Token consumption helpers

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw new RuntimeException(message, Peek().Line);
    }

    private Token ConsumeMatching(TokenType type, string expected)
    {
        if (Check(type)) return Advance();
        return null;
    }

    private bool Match(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }
        return false;
    }

    private TokenType MatchPrevType
    {
        get
        {
            if (_current > 0) return _tokens[_current - 1].Type;
            return TokenType.EOF;
        }
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd() => Peek().Type == TokenType.EOF;

    private Token Peek() => _tokens[_current];

    private Token Previous() => _tokens[_current - 1];

    private void Synchronize()
    {
        if (!IsAtEnd()) Advance();
        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Newline) return;

            switch (Peek().Type)
            {
                case TokenType.Var:
                case TokenType.Const:
                case TokenType.Local:
                case TokenType.Function:
                case TokenType.Class:
                case TokenType.Blueprint:
                case TokenType.If:
                case TokenType.While:
                case TokenType.For:
                case TokenType.RepeatH:
                case TokenType.Match:
                case TokenType.Return:
                case TokenType.SendBack:
                case TokenType.Import:
                case TokenType.Break:
                case TokenType.StopH:
                case TokenType.Continue:
                case TokenType.SkipH:
                case TokenType.End:
                    return;
            }
            Advance();
        }
    }
}
