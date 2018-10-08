using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public abstract partial class Declaration : Locatable , IPretty
    {
        public static Dictionary<Type, string> types = new Dictionary<Type, string> {
            {Type.INT, "int"}, {Type.BOOL, "bool"}, {Type.VOID, "void"}
        };
        virtual public void Pretty(PrettyBuilder builder)
        {

        }
    }

    public partial class SequenceDeclaration : Declaration
    {
        override public void Pretty(PrettyBuilder builder)
        {
            head.Pretty(builder);
            tail.Pretty(builder);
        }
    }

    public partial class FunctionDeclaration : Declaration
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.NewLine();
            builder.Append(types[type] + " ");
            builder.Append(id.id);
            builder.Append("(");
            builder.Intersperse(arg, ", ");
            builder.Append(")");

            stm.Pretty(builder);

            //builder.NewLine();
        }
    }

    public partial class TypeDeclaration : Declaration
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.Append(types[type] + " ");
            builder.Append(id.id);
        }
    }


    //statements

    public abstract partial class Statement : Locatable
    {
        virtual public void Pretty(PrettyBuilder builder)
        {

        }
    }

    public partial class SequenceStatement : Statement
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.PrettyStm(stmList);
        }
    }

    public partial class BlockStatement : Statement
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.NewLine();
            builder.Append("{");
            builder.Indent();
            builder.PrettyStm(stmList);
            builder.Unindent();
            builder.NewLine();
            builder.Append("}");
        }
    }

    public partial class ifStatement : Statement
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.NewLine();
            builder.Append("if(");
            exp.Pretty(builder);
            builder.Append(")");
            //builder.Indent();
            stm.Pretty(builder);
            //builder.Unindent();
            //builder.NewLine();
        }
    }

    public partial class ifElseStatement : Statement
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.NewLine();
            builder.Append("if(");
            exp.Pretty(builder);
            builder.Append(")");
            //builder.Indent();
            ifstm.Pretty(builder);
            //builder.Unindent();
            builder.NewLine();
            builder.Append("else");
            //builder.Indent();
            elsestm.Pretty(builder);
            //builder.Unindent();
        }
    }

    public partial class whileStatement : Statement
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.NewLine();
            builder.Append("while(");
            exp.Pretty(builder);
            builder.Append(")");
            //builder.Indent();
            stm.Pretty(builder);
            //builder.Unindent();
            //builder.NewLine();
        }
    }

    public partial class returnExprStatement : Statement
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.NewLine();
            builder.Append("return ");
            returnValue.Pretty(builder);
            builder.Append(";");
        }
    }

    public partial class returnStatement : Statement
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.NewLine();
            builder.Append("return;");
        }
    }

    public partial class ExprStatement : Statement
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.NewLine();
            exp.Pretty(builder);
            builder.Append(";");
        }
    }

    public partial class DeclStatement : Statement
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.NewLine();
            decl.Pretty(builder);
            builder.Append(";");
        }
    }

    public abstract partial class Expression : Locatable
    {
        virtual public void Pretty(PrettyBuilder builder)
        {
            
        }
        virtual public void Pretty(PrettyBuilder builder, int outerPrecedence, bool opposite)
        {
            Pretty(builder);
        }
    }

    public partial class IdentifierExpression : Expression
    {
            override public void Pretty(PrettyBuilder builder)
            {
                builder.Append(id);
            }
        }

    public partial class NumberExpression : Expression
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.Append(num.ToString());
        }
    }

    public partial class BinaryOperatorExpression : Expression
    {

        public static Dictionary<Operator, int> precedences = new Dictionary<Operator, int>
        {
            {Operator.ADD, 6}, {Operator.MINUS, 6},
            {Operator.DIV, 7}, {Operator.MULTI, 7},
            {Operator.ASN, 1}, {Operator.OR, 2},
            {Operator.AND, 3}, {Operator.EQ, 4},
            {Operator.NOTEQ, 4}, {Operator.LESS, 5},
            {Operator.MORE, 5}, {Operator.LESSEQ, 5},
            {Operator.MOREEQ, 5}
        };

        public enum Associativity { LEFT, RIGHT }

        public static Dictionary<Operator, string> operators = new Dictionary<Operator, string> {
            {Operator.ADD, "+"}, {Operator.MINUS, "-"},
            {Operator.DIV, "/"}, {Operator.MULTI, "*"},
            {Operator.ASN, "="}, {Operator.OR, "||"},
            {Operator.AND, "&&"}, {Operator.EQ, "=="},
            {Operator.NOTEQ, "!="}, {Operator.LESS, "<"},
            {Operator.MORE, ">"}, {Operator.LESSEQ, "<="},
            {Operator.MOREEQ, ">="}
        };

        override public void Pretty(PrettyBuilder builder)
        {
            Pretty(builder, 0, false);
        }
        override public void Pretty(PrettyBuilder builder, int outerPrecedence, bool opposite)
        {
            bool eval = outerPrecedence > precedences[op] || opposite && outerPrecedence == precedences[op];
            if (eval) builder.Append("(");
            left.Pretty(builder, precedences[op], false);
            builder.Append(" " + operators[op] + " ");
            right.Pretty(builder, precedences[op], true);
            if (eval) builder.Append(")");
        }
    }

    public partial class AssignmentExpression : Expression
    {
        override public void Pretty(PrettyBuilder builder)
        {
            lo.Pretty(builder);
            builder.Append(" = ");
            ro.Pretty(builder);
        }
        override public void Pretty(PrettyBuilder builder, int outerPrecedence, bool opposite)
        {
            bool eval = outerPrecedence > 1 || opposite && outerPrecedence == 1;
            if (eval) builder.Append("(");
            lo.Pretty(builder);
            builder.Append(" = ");
            ro.Pretty(builder);
            if (eval) builder.Append(")");
        }
    }

    public partial class BoolExpression : Expression
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.Append(binvalue.ToString().ToLower());
        }
    }

    public partial class UnaryOperatorExpression : Expression
    {
        static Dictionary<Operator, string> operators = new Dictionary<Operator, string> {
            {Operator.MINUS, "-"},
            {Operator.EXCLA, "!"}
        };
        override public void Pretty(PrettyBuilder builder)
        {
            Pretty(builder, 8, false);
        }
        override public void Pretty(PrettyBuilder builder, int outerPrecedence, bool opposite)
        {
            BinaryOperatorExpression tempB;
            UnaryOperatorExpression tempU;
            AssignmentExpression tempA;
            bool eval = false;
            System.Type type = exp.GetType();
            if(type.Name == "UnaryOperatorExpression")
            {
                tempU = (UnaryOperatorExpression)exp;
                eval = outerPrecedence > 8;
            }
            if(type.Name == "BinaryOperatorExpression")
            {
                tempB = (BinaryOperatorExpression)exp;
                eval = outerPrecedence > BinaryOperatorExpression.precedences[tempB.op];
            }
            if (type.Name == "AssignmentExpression")
            {
                tempA = (AssignmentExpression)exp;
                eval = outerPrecedence >= 1;
            }
            builder.Append(operators[op]);

            exp.Pretty(builder, 8, false);

        }
    }

    public partial class FunctionCallExpression : Expression
    {
        override public void Pretty(PrettyBuilder builder)
        {
            builder.Append(id.id);
            builder.Append("(");
            builder.Intersperse(exprList, ", ");
            builder.Append(")");
        }
    }

}

