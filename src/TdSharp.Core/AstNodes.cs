namespace TdSharp.Core;

public abstract class Expr
{
    public interface IVisitor<T>
    {
        T VisitBinaryExpr(Binary expr);
        T VisitUnaryExpr(Unary expr);
        T VisitLiteralExpr(Literal expr);
        T VisitGroupingExpr(Grouping expr);
        T VisitVariableExpr(Variable expr);
        T VisitAssignExpr(Assign expr);
        T VisitLogicalExpr(Logical expr);
        T VisitCallExpr(Call expr);
        T VisitGetExpr(Get expr);
        T VisitSetExpr(Set expr);
        T VisitArrayExpr(ArrayExpr expr);
        T VisitMapExpr(MapExpr expr);
        T VisitIndexExpr(Index expr);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);

    public class Binary : Expr
    {
        public Expr Left { get; }
        public Token Op { get; }
        public Expr Right { get; }

        public Binary(Expr left, Token op, Expr right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBinaryExpr(this);
    }

    public class Unary : Expr
    {
        public Token Op { get; }
        public Expr Right { get; }

        public Unary(Token op, Expr right)
        {
            Op = op;
            Right = right;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitUnaryExpr(this);
    }

    public class Literal : Expr
    {
        public TdValue Value { get; }

        public Literal(TdValue value)
        {
            Value = value;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLiteralExpr(this);
    }

    public class Grouping : Expr
    {
        public Expr Expression { get; }

        public Grouping(Expr expression)
        {
            Expression = expression;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitGroupingExpr(this);
    }

    public class Variable : Expr
    {
        public Token Name { get; }

        public Variable(Token name)
        {
            Name = name;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitVariableExpr(this);
    }

    public class Assign : Expr
    {
        public Token Name { get; }
        public Expr Value { get; }

        public Assign(Token name, Expr value)
        {
            Name = name;
            Value = value;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitAssignExpr(this);
    }

    public class Logical : Expr
    {
        public Expr Left { get; }
        public Token Op { get; }
        public Expr Right { get; }

        public Logical(Expr left, Token op, Expr right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLogicalExpr(this);
    }

    public class Call : Expr
    {
        public Expr Callee { get; }
        public Token Paren { get; }
        public List<Expr> Arguments { get; }

        public Call(Expr callee, Token paren, List<Expr> arguments)
        {
            Callee = callee;
            Paren = paren;
            Arguments = arguments;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitCallExpr(this);
    }

    public class Get : Expr
    {
        public Expr Object { get; }
        public Token Name { get; }

        public Get(Expr obj, Token name)
        {
            Object = obj;
            Name = name;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitGetExpr(this);
    }

    public class Set : Expr
    {
        public Expr Object { get; }
        public Token Name { get; }
        public Expr Value { get; }

        public Set(Expr obj, Token name, Expr value)
        {
            Object = obj;
            Name = name;
            Value = value;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitSetExpr(this);
    }

    public class ArrayExpr : Expr
    {
        public List<Expr> Elements { get; }

        public ArrayExpr(List<Expr> elements)
        {
            Elements = elements;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitArrayExpr(this);
    }

    public class MapExpr : Expr
    {
        public List<(Token Key, Expr Value)> Entries { get; }

        public MapExpr(List<(Token Key, Expr Value)> entries)
        {
            Entries = entries;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitMapExpr(this);
    }

    public class Index : Expr
    {
        public Expr Object { get; }
        public Expr IndexExpr { get; }

        public Index(Expr obj, Expr indexExpr)
        {
            Object = obj;
            IndexExpr = indexExpr;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitIndexExpr(this);
    }
}

public abstract class Stmt
{
    public interface IVisitor<T>
    {
        T VisitExpressionStmt(Expression stmt);
        T VisitVarStmt(Var stmt);
        T VisitConstStmt(Const stmt);
        T VisitIfStmt(If stmt);
        T VisitWhileStmt(While stmt);
        T VisitForStmt(For stmt);
        T VisitForEachStmt(ForEach stmt);
        T VisitMatchStmt(Match stmt);
        T VisitFunctionStmt(Function stmt);
        T VisitClassStmt(Class stmt);
        T VisitReturnStmt(Return stmt);
        T VisitBreakStmt(Break stmt);
        T VisitContinueStmt(Continue stmt);
        T VisitBlockStmt(Block stmt);
        T VisitImportStmt(Import stmt);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);

    public class Expression : Stmt
    {
        public Expr Expr { get; }

        public Expression(Expr expr)
        {
            Expr = expr;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitExpressionStmt(this);
    }

    public class Var : Stmt
    {
        public Token Name { get; }
        public Expr Initializer { get; }
        public bool IsLocal { get; }

        public Var(Token name, Expr initializer, bool isLocal = false)
        {
            Name = name;
            Initializer = initializer;
            IsLocal = isLocal;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitVarStmt(this);
    }

    public class Const : Stmt
    {
        public Token Name { get; }
        public Expr Initializer { get; }

        public Const(Token name, Expr initializer)
        {
            Name = name;
            Initializer = initializer;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitConstStmt(this);
    }

    public class If : Stmt
    {
        public Expr Condition { get; }
        public List<Stmt> ThenBranch { get; }
        public List<Stmt> ElifBranches { get; }   // list of (condition, body)
        public List<Stmt> ElseBranch { get; }

        public If(Expr condition, List<Stmt> thenBranch, List<Stmt> elifBranches, List<Stmt> elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElifBranches = elifBranches;
            ElseBranch = elseBranch;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitIfStmt(this);
    }

    public class While : Stmt
    {
        public Expr Condition { get; }
        public List<Stmt> Body { get; }

        public While(Expr condition, List<Stmt> body)
        {
            Condition = condition;
            Body = body;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitWhileStmt(this);
    }

    public class For : Stmt
    {
        public Token Variable { get; }
        public Expr Start { get; }
        public Expr End { get; }
        public List<Stmt> Body { get; }

        public For(Token variable, Expr start, Expr end, List<Stmt> body)
        {
            Variable = variable;
            Start = start;
            End = end;
            Body = body;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitForStmt(this);
    }

    public class ForEach : Stmt
    {
        public Token Variable { get; }
        public Expr Collection { get; }
        public List<Stmt> Body { get; }

        public ForEach(Token variable, Expr collection, List<Stmt> body)
        {
            Variable = variable;
            Collection = collection;
            Body = body;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitForEachStmt(this);
    }

    public class Match : Stmt
    {
        public Expr Value { get; }
        public List<(Expr Pattern, List<Stmt> Body)> Cases { get; }
        public List<Stmt> ElseBody { get; }

        public Match(Expr value, List<(Expr Pattern, List<Stmt> Body)> cases, List<Stmt> elseBody)
        {
            Value = value;
            Cases = cases;
            ElseBody = elseBody;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitMatchStmt(this);
    }

    public class Function : Stmt
    {
        public Token Name { get; }
        public List<Token> Parameters { get; }
        public List<Stmt> Body { get; }

        public Function(Token name, List<Token> parameters, List<Stmt> body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitFunctionStmt(this);
    }

    public class Class : Stmt
    {
        public Token Name { get; }
        public string InheritsFrom { get; }
        public List<Function> Methods { get; }
        public Function Constructor { get; }

        public Class(Token name, string inheritsFrom, List<Function> methods, Function constructor)
        {
            Name = name;
            InheritsFrom = inheritsFrom;
            Methods = methods;
            Constructor = constructor;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitClassStmt(this);
    }

    public class Return : Stmt
    {
        public Token Keyword { get; }
        public Expr Value { get; }

        public Return(Token keyword, Expr value)
        {
            Keyword = keyword;
            Value = value;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitReturnStmt(this);
    }

    public class Break : Stmt
    {
        public Token Keyword { get; }

        public Break(Token keyword)
        {
            Keyword = keyword;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBreakStmt(this);
    }

    public class Continue : Stmt
    {
        public Token Keyword { get; }

        public Continue(Token keyword)
        {
            Keyword = keyword;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitContinueStmt(this);
    }

    public class Block : Stmt
    {
        public List<Stmt> Statements { get; }

        public Block(List<Stmt> statements)
        {
            Statements = statements;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBlockStmt(this);
    }

    public class Import : Stmt
    {
        public Token Keyword { get; }
        public Expr Path { get; }

        public Import(Token keyword, Expr path)
        {
            Keyword = keyword;
            Path = path;
        }

        public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitImportStmt(this);
    }
}
