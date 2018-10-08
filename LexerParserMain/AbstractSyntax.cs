using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    // declarations

    public abstract partial class Declaration : Locatable, IPretty
    {
        public enum Type { INT, BOOL, NULL, VOID }
    }

    public partial class SequenceDeclaration : Declaration
    {
        public Declaration head;
        public Declaration tail;

        public SequenceDeclaration(Declaration head, Declaration tail)
        {
            this.head = head;
            this.tail = tail;
        }
    }

    public partial class FunctionDeclaration : Declaration
    {
        public Type type;
        public IdentifierExpression id;
        public List<TypeDeclaration> arg;
        public Statement stm;

        public FunctionDeclaration(Type type, IdentifierExpression id, List<TypeDeclaration> arg, Statement stm)
        {
            this.type = type;
            this.id = id;
            this.arg = arg;
            this.stm = stm;
        }
    }

    public partial class TypeDeclaration : Declaration
    {
        public Type type;
        public IdentifierExpression id;

        public TypeDeclaration(Type type, IdentifierExpression id)
        {
            this.type = type;
            this.id = id;
        }
    }


    //statements

    public abstract partial class Statement : Locatable, IPretty
    {

    }

    public partial class SequenceStatement : Statement
    {
        //public Statement head;
        public List<Statement> stmList;

        public SequenceStatement(List<Statement> stmList)
        {
            this.stmList = stmList;
        }
    }

    public partial class BlockStatement : Statement
    {
        public List<Statement> stmList;

        public BlockStatement(List<Statement> stmList)
        {
            this.stmList = stmList;
        }
    }

    public partial class ifStatement : Statement
    {
        public Expression exp;
        public Statement stm;

        public ifStatement(Expression exp, Statement stm)
        {
            this.exp = exp;
            this.stm = stm;
        }
    }

    public partial class ifElseStatement : Statement
    {
        public Expression exp;
        public Statement ifstm;
        public Statement elsestm;

        public ifElseStatement(Expression exp, Statement ifstm, Statement elsestm)
        {
            this.exp = exp;
            this.ifstm = ifstm;
            this.elsestm = elsestm;
        }
    }

    public partial class whileStatement : Statement
    {
        public Expression exp;
        public Statement stm;

        public whileStatement(Expression exp, Statement stm)
        {
            this.exp = exp;
            this.stm = stm;
        }
    }

    public partial class returnExprStatement : Statement
    {
        public Expression returnValue;
        public Declaration.Type type;

        public returnExprStatement(Expression returnValue)
        {
            this.returnValue = returnValue;
        }
    }

    public partial class returnStatement : Statement
    {
        public returnStatement()
        {
            
        }
    }

    public partial class ExprStatement : Statement
    {
        public Expression exp;

        public ExprStatement(Expression exp)
        {
            this.exp = exp;
        }
    }

    public partial class DeclStatement : Statement
    {
        public TypeDeclaration decl;
        public DeclStatement(TypeDeclaration decl)
        {
            this.decl = decl;
        }
    }


    public abstract partial class Expression : Locatable, IPretty
    {

    }

    public partial class IdentifierExpression : Expression
    {
        public string id;
        public Declaration.Type type;

        public IdentifierExpression(string id)
        {
            this.id = id;
        }
    }

    public partial class NumberExpression : Expression
    {
        public int num;

        public NumberExpression(string num)
        {
            this.num = Convert.ToInt32(num);
        }
    }

    public partial class BinaryOperatorExpression : Expression
    {
        public Operator op;
        public Expression left;
        public Expression right;
        public Declaration.Type type;

        public enum Operator { ADD, MINUS, MULTI, DIV, ASN, OR, AND, EQ, NOTEQ, LESS, MORE, LESSEQ, MOREEQ }
        public BinaryOperatorExpression(Operator op, Expression left, Expression right)
        {
            this.op = op;
            this.left = left;
            this.right = right;
        }
    }

    public partial class AssignmentExpression : Expression
    {
        public Expression lo;
        public Expression ro;
        public Declaration.Type type;
        public AssignmentExpression(Expression lo, Expression ro)
        {
            this.lo = lo;
            this.ro = ro;
        }
    }

    public partial class BoolExpression : Expression
    {
        public bool binvalue;
        
        public BoolExpression(bool binValue)
        {
            binvalue = binValue;
        }
    }

    public partial class UnaryOperatorExpression : Expression
    {
        public Operator op;
        public Expression exp;

        public enum Operator { MINUS, EXCLA }

        public UnaryOperatorExpression(Operator op, Expression exp)
        {
            this.op = op;
            this.exp = exp;
        }
    }

    public partial class FunctionCallExpression : Expression
    {
        public IdentifierExpression id;
        public List<Expression> exprList;
        public List<Declaration.Type> types = new List<Declaration.Type>();

        public FunctionCallExpression(IdentifierExpression id, List<Expression> exprList)
        {
            this.id = id;
            this.exprList = exprList;
        }
    }


    public partial class Locatable
    {
        public int line;
        public int column;
        public void SetLocation(QUT.Gppg.LexLocation loc)
        {
            this.line = loc.StartLine;
            this.column = loc.StartColumn + 1;
        }
    }


}

