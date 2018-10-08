using System;
using System.Collections.Generic;

namespace Parser
{

    public interface IDeclarationVisitor<R, A>
    {
        R Visit(SequenceDeclaration s, A arg);
        R Visit(FunctionDeclaration s, A arg);
        R Visit(TypeDeclaration s, A arg);
    }

    public interface IStatementVisitor<R, A>
    {
        R Visit(SequenceStatement s, A arg);
        R Visit(BlockStatement s, A arg);
        R Visit(ifStatement s, A arg);
        R Visit(ifElseStatement s, A arg);
        R Visit(whileStatement s, A arg);
        R Visit(returnExprStatement s, A arg);
        R Visit(returnStatement s, A arg);
        R Visit(ExprStatement s, A arg);
        R Visit(DeclStatement s, A arg);
    }

    public interface IExpressionVisitor<R, A>
    {
        R Visit(IdentifierExpression e, A arg);
        R Visit(NumberExpression e, A arg);
        R Visit(BinaryOperatorExpression e, A arg);
        R Visit(AssignmentExpression e, A arg);
        R Visit(BoolExpression e, A arg);
        R Visit(UnaryOperatorExpression e, A arg);
        R Visit(FunctionCallExpression e, A arg);
    }


    public abstract partial class Declaration : Locatable, IPretty
    {
        public abstract R Accept<R, A>(IDeclarationVisitor<R, A> v, A arg);
    }

    public partial class SequenceDeclaration : Declaration
    {
        override public R Accept<R, A>(IDeclarationVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class FunctionDeclaration : Declaration
    {
        override public R Accept<R, A>(IDeclarationVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class TypeDeclaration : Declaration
    {
        override public R Accept<R, A>(IDeclarationVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }

    public abstract partial class Statement : Locatable, IPretty
    {
        public abstract R Accept<R, A>(IStatementVisitor<R, A> v, A arg);
    }

    public partial class SequenceStatement : Statement
    {
        override public R Accept<R, A>(IStatementVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class BlockStatement : Statement
    {
        override public R Accept<R, A>(IStatementVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class whileStatement : Statement
    {
        override public R Accept<R, A>(IStatementVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class ifElseStatement : Statement
    {
        override public R Accept<R, A>(IStatementVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class ifStatement : Statement
    {
        override public R Accept<R, A>(IStatementVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class returnStatement : Statement
    {
        override public R Accept<R, A>(IStatementVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class returnExprStatement : Statement
    {
        override public R Accept<R, A>(IStatementVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class DeclStatement : Statement
    {
        override public R Accept<R, A>(IStatementVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class ExprStatement : Statement
    {
        override public R Accept<R, A>(IStatementVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }


    public abstract partial class Expression : Locatable, IPretty
    {
        public abstract R Accept<R, A>(IExpressionVisitor<R, A> v, A arg);
    }

    public partial class IdentifierExpression : Expression
    {
        override public R Accept<R, A>(IExpressionVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class NumberExpression : Expression
    {
        override public R Accept<R, A>(IExpressionVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class BinaryOperatorExpression : Expression
    {
        override public R Accept<R, A>(IExpressionVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class AssignmentExpression : Expression
    {
        override public R Accept<R, A>(IExpressionVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class BoolExpression : Expression
    {
        override public R Accept<R, A>(IExpressionVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class UnaryOperatorExpression : Expression
    {
        override public R Accept<R, A>(IExpressionVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
    public partial class FunctionCallExpression : Expression
    {
        override public R Accept<R, A>(IExpressionVisitor<R, A> v, A arg)
        {
            return v.Visit(this, arg);
        }
    }
}