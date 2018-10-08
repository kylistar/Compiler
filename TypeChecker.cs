using System;
using System.Collections.Generic;
using Parser;

namespace TypeChecker
{
    public class TypeCheckerException : Exception
    {
        public TypeCheckerException(string msg, Locatable l) : base(MkMessage(msg, l))
        {
        }

        public static string MkMessage(string msg, Locatable l)
        {
            return string.Format("fail {0} {1}", l.line, l.column);
        }
    }

    public class Environment
    {
        public Dictionary<Type, Declaration.Type> typeDict = new Dictionary<Type, Declaration.Type>
            {
                {typeof(int),Declaration.Type.INT},
                {typeof(bool),Declaration.Type.BOOL}
            };
        Dictionary<string, Object> varEnv = new Dictionary<string, Object>();
        public Dictionary<string, FunctionDeclaration> functionDictionary = new Dictionary<string, FunctionDeclaration>();
        public Dictionary<string, Declaration.Type> varTypeDict = new Dictionary<string, Declaration.Type>();
        Dictionary<string, Declaration.Type> funcTypeDict = new Dictionary<string, Declaration.Type>();
        public Declaration.Type typeOfFunc;
        public bool mainBlock = false;
        Environment parentEnvironment = null, mainEnv = null;
        public Environment ParentEnvironment { get => parentEnvironment; set => parentEnvironment = value; }
        public Environment MainEnv { get => mainEnv; set => mainEnv = value; }
        public Dictionary<string, Declaration.Type> FuncTypeDict { get => funcTypeDict; set => funcTypeDict = value; }


        public Declaration.Type Lookup(string x, Environment env)
        {
            if (env.varTypeDict.ContainsKey(x))
            {
                return env.varTypeDict[x];
            }
            else if (!env.varTypeDict.ContainsKey(x) && env.ParentEnvironment != null)
                return Lookup(x, env.ParentEnvironment);

            throw new NotSupportedException();
        }
        public void Create(TypeDeclaration x)
        {
            varTypeDict[x.id.id] = x.type;
        }
        public void addFunction(Object f)
        {
            FunctionDeclaration t = (FunctionDeclaration)f;
            functionDictionary[t.id.id] = t;
        }
        public void addFuncType(string x, Declaration.Type type)
        {
            FuncTypeDict[x] = type;
        }
        public FunctionDeclaration getFunction(string id)
        {
            if (functionDictionary.ContainsKey(id))
                return functionDictionary[id];
            else
            {
                return null;
            }
        }
        public bool Has(string x, Environment env)
        {
            var r = env.varTypeDict.ContainsKey(x);
            if (!r && env.ParentEnvironment != null)
                r = Has(x, env.ParentEnvironment);

            if (r)
                return r;
            else
                return false;
        }
        public Object getVarType(string x, Environment env)
        {
            var r = env.varTypeDict.ContainsKey(x);
            if (r)
            {
                return env.varTypeDict[x];
            }

            if (!r && env.ParentEnvironment != null)
            {
                return getVarType(x, env.ParentEnvironment);
            }

            return null;
        }
    }


    public class ProgramTypeChecker
    {
        Environment mainEnvironment = new Environment();
        DeclarationTypeChecker sev = new DeclarationTypeChecker();
        public ProgramTypeChecker(Parser.Parser p)
        {
            mainEnvironment.MainEnv = mainEnvironment;
            if(p.program.GetType() == typeof(SequenceDeclaration))
                addFunctions((SequenceDeclaration)p.program);
            p.program.Accept(sev, mainEnvironment);
        }
        public void addFunctions(SequenceDeclaration p)
        {
            if (p.head.GetType() == typeof(SequenceDeclaration))
            {
                addFunctions((SequenceDeclaration)p.head); // s.head.Accept(this, env);
            }

            if (p.tail.GetType() == typeof(SequenceDeclaration))
            {
                addFunctions((SequenceDeclaration)p.tail); // s.head.Accept(this, env);
            }

            if(p.head.GetType() == typeof(FunctionDeclaration))
                mainEnvironment.addFunction(p.head);

            if (p.tail.GetType() == typeof(FunctionDeclaration))
                mainEnvironment.addFunction(p.tail);
        }
    }

    public class DeclarationTypeChecker : IDeclarationVisitor<Object, Environment>
    {
        static StatementTypeChecker sv = new StatementTypeChecker();
        static ExpressionTypeChecker ev = new ExpressionTypeChecker();


        public Object Visit(SequenceDeclaration s, Environment env)
        {
            s.head.Accept(this, env);
            s.tail.Accept(this, env);
            return null;
        }
        public Object Visit(FunctionDeclaration s, Environment env)
        {
            StatementTypeChecker se = new StatementTypeChecker();
            Environment varEnvironment = new Environment();
            varEnvironment.typeOfFunc = s.type;
            varEnvironment.mainBlock = true;

            foreach (var e in s.arg)
            {
                varEnvironment.Create(e);
            }
            varEnvironment.functionDictionary = env.functionDictionary;
            varEnvironment.MainEnv = env.MainEnv;
            var r = s.stm.Accept(se, varEnvironment);
            if(r == null) { r = Declaration.Type.VOID; }
            if (s.type != (Declaration.Type)r)
            {
                throw new TypeCheckerException("Function " + s.id.id + " does not return the correct type", s);
            }
            if (s.type == (Declaration.Type)r)
            {
                env.addFuncType(s.id.id, s.type);
                return null;
            }
            else
                throw new TypeCheckerException("Function " + s.id.id + " does not return the correct type", s);
        }
        public Object Visit(TypeDeclaration s, Environment env)
        {
            env.Create(s);
            return null;
        }
    }

    public class StatementTypeChecker : IStatementVisitor<Object, Environment>
    {
        static ExpressionTypeChecker ev = new ExpressionTypeChecker();
        static DeclarationTypeChecker dv = new DeclarationTypeChecker();

        public Object Visit(SequenceStatement s, Environment env)
        {
            foreach (var e in s.stmList)
            {
                e.Accept(this, env);
            }
            return null;
        }
        public Object Visit(returnExprStatement s, Environment env)
        {
            if(env.typeOfFunc == Declaration.Type.VOID && env.ParentEnvironment != null)
            {
                return Visit(s, env.ParentEnvironment);
            }
            var r = s.returnValue.Accept(ev, env);
            if (r != env.typeOfFunc)
            {
                throw new TypeCheckerException("F", s);
            }
            s.type = r;
            return r;
        }
        public Object Visit(returnStatement s, Environment env)
        {
            if (env.typeOfFunc != Declaration.Type.VOID)
            {
                throw new TypeCheckerException("F", s);
            }
            return Declaration.Type.VOID;
        }
        public Object Visit(DeclStatement s, Environment env)
        {
            s.decl.Accept(dv, env);
            return null;
        }
        public Object Visit(BlockStatement s, Environment env)
        {
            Dictionary<string, Environment> envChoice = new Dictionary<string, Environment>();
            Environment tempEnv = new Environment();
            Declaration.Type returnTypen = Declaration.Type.NULL;

            if (env.mainBlock == false)
            {
                envChoice["env"] = tempEnv;
                envChoice["env"].ParentEnvironment = env;
                envChoice["env"].MainEnv = env.MainEnv;
                envChoice["env"].typeOfFunc = Declaration.Type.VOID;
            }
            else
            {
                envChoice["env"] = env;
                env.mainBlock = false;
            }
            var t = envChoice["env"].typeOfFunc;
            foreach (var e in s.stmList)
            {
                var returntype = e.Accept(this, envChoice["env"]);
                if (returntype != null)
                {
                    env.mainBlock = true;
                    returnTypen = (Declaration.Type)returntype;
                }
            }
            if (returnTypen != Declaration.Type.NULL)
                return returnTypen;

            return null;

            throw new NotSupportedException();
        }
        public Object Visit(ifStatement s, Environment env)
        {
            var v = s.exp.Accept(ev, env);
            if (v != Declaration.Type.BOOL)
            {
                throw new TypeCheckerException("If-statement expects a boolean condition", s);
            }
            return s.stm.Accept(this, env);
        }
        public Object Visit(whileStatement s, Environment env)
        {
            if(s.exp.Accept(ev, env) != Declaration.Type.BOOL)
            {
                throw new TypeCheckerException("While-statement expects a boolean condition", s);
            }
            return s.stm.Accept(this, env);
        }
        public Object Visit(ExprStatement s, Environment env)
        {
            s.exp.Accept(ev, env);
            return null;
        }
        public Object Visit(ifElseStatement s, Environment env)
        {
            var v = s.exp.Accept(ev, env);
            if (v != Declaration.Type.BOOL)
            {
                throw new TypeCheckerException("If-else-statement expects a boolean condition", s);
            }
            var returnvalue = s.ifstm.Accept(this, env);
            var returnvalue2 = s.elsestm.Accept(this, env);
            if (returnvalue != null && returnvalue2 != null)
            {
                if (!returnvalue.Equals(returnvalue2) && (returnvalue != null || returnvalue2 != null))
                {
                    throw new TypeCheckerException("different return types in if-else-statement", s);
                }
            }
            if (returnvalue != null)
                return returnvalue;
            else
                return returnvalue2;
        }
    }

    public class ExpressionTypeChecker : IExpressionVisitor<Declaration.Type, Environment>
    {
        public Declaration.Type Visit(IdentifierExpression e, Environment env)
        {
            if (!env.Has(e.id, env))
            {
                throw new TypeCheckerException(string.Format("Variable {0} not defined", e.id), e);
            }
            e.type = (Declaration.Type)env.getVarType(e.id, env);

            return env.Lookup(e.id, env);
        }
        public Declaration.Type Visit(NumberExpression e, Environment env)
        {
            return Declaration.Type.INT;
        }
        public Declaration.Type Visit(BoolExpression s, Environment env)
        {
            return Declaration.Type.BOOL;
        }
        public Declaration.Type Visit(BinaryOperatorExpression e, Environment env)
        {
            var vl = e.left.Accept(this, env);
            var vr = e.right.Accept(this, env);
            e.type = vl;
            switch (e.op)
            {
                case BinaryOperatorExpression.Operator.ADD:
                    if (vl != Declaration.Type.INT || vr != Declaration.Type.INT)
                    {
                        throw new TypeCheckerException("Type mismatch in addition", e);
                    }
                    return Declaration.Type.INT;
                case BinaryOperatorExpression.Operator.MINUS:
                    if (vl != Declaration.Type.INT || vr != Declaration.Type.INT)
                    {
                        throw new TypeCheckerException("Type mismatch in subtraction", e);
                    }
                    return Declaration.Type.INT;
                case BinaryOperatorExpression.Operator.MULTI:
                    if (vl != Declaration.Type.INT || vr != Declaration.Type.INT)
                    {
                        throw new TypeCheckerException("Type mismatch in multiplication", e);
                    }
                    return Declaration.Type.INT;
                case BinaryOperatorExpression.Operator.DIV:
                    if (vl != Declaration.Type.INT || vr != Declaration.Type.INT)
                    {
                        throw new TypeCheckerException("Type mismatch in division", e);
                    }
                    return Declaration.Type.INT;
                case BinaryOperatorExpression.Operator.AND:
                    if (vl != Declaration.Type.BOOL || vr != Declaration.Type.BOOL)
                    {
                        throw new TypeCheckerException("Type mismatch in boolean operation", e);
                    }
                    return Declaration.Type.BOOL;
                case BinaryOperatorExpression.Operator.LESS:
                    if (vl != Declaration.Type.INT || vr != Declaration.Type.INT)
                    {
                        throw new TypeCheckerException("Type mismatch in boolean operation", e);
                    }
                    return Declaration.Type.BOOL;
                case BinaryOperatorExpression.Operator.MORE:
                    if (vl != Declaration.Type.INT || vr != Declaration.Type.INT)
                    {
                        throw new TypeCheckerException("Type mismatch in boolean operation", e);
                    }
                    return Declaration.Type.BOOL;
                case BinaryOperatorExpression.Operator.MOREEQ:
                    if (vl != Declaration.Type.INT || vr != Declaration.Type.INT)
                    {
                        throw new TypeCheckerException("Type mismatch in boolean operation", e);
                    }
                    return Declaration.Type.BOOL;
                case BinaryOperatorExpression.Operator.LESSEQ:
                    if (vl != Declaration.Type.INT || vr != Declaration.Type.INT)
                    {
                        throw new TypeCheckerException("Type mismatch in boolean operation", e);
                    }
                    return Declaration.Type.BOOL;
                case BinaryOperatorExpression.Operator.EQ:
                    if (vl != vr)
                    {
                        throw new TypeCheckerException("Type mismatch in boolean operation", e);
                    }
                    return Declaration.Type.BOOL;
                case BinaryOperatorExpression.Operator.NOTEQ:
                    if (vl != vr)
                    {
                        throw new TypeCheckerException("Type mismatch in boolean operation", e);
                    }
                    return Declaration.Type.BOOL;
                case BinaryOperatorExpression.Operator.OR:
                    if (vl != vr)
                    {
                        throw new TypeCheckerException("Type mismatch in boolean operation", e);
                    }
                    return Declaration.Type.BOOL;
            }
            throw new TypeCheckerException("Something went wrong in expression", e);
        }
        public Declaration.Type Visit(AssignmentExpression s, Environment env)
        {

            IdentifierExpression temp1;
            AssignmentExpression temp2;
            var v = s.ro.Accept(this, env);

            if (s.lo.GetType() == typeof(IdentifierExpression))
            {
                temp1 = (IdentifierExpression)s.lo;
                if (!env.Has(temp1.id, env)){
                    throw new TypeCheckerException("Assignment of undeclared variable", s);
                }
                if (env.Lookup(temp1.id, env) != v)
                {
                    throw new TypeCheckerException("Type mismatch in assigment", s);
                }
            }
            if (s.lo.GetType() == typeof(AssignmentExpression))
            {
                temp2 = (AssignmentExpression)s.lo;
                temp1 = (IdentifierExpression)temp2.ro;
                if (env.Lookup(temp1.id, env) != v)
                {
                    throw new TypeCheckerException("Type mismatch in assigment", s);
                }
                temp2.Accept(this, env);
            }
            s.type = v;
            return 0;
        }
        public Declaration.Type Visit(UnaryOperatorExpression s, Environment env)
        {
            var v = s.exp.Accept(this, env);
            if (s.op == UnaryOperatorExpression.Operator.EXCLA)
            {
                if(v != Declaration.Type.BOOL)
                {
                    throw new TypeCheckerException("Unary operator ! expects a boolean expression", s);
                }
                return Declaration.Type.BOOL;
            }
            else
            {
                if (v != Declaration.Type.INT)
                {
                    throw new TypeCheckerException("Unary operator - expects an integer expression", s);
                }
                return Declaration.Type.INT;
            }
        }
        public Declaration.Type Visit(FunctionCallExpression s, Environment env)
        {
            if (s.id.id.Equals("print"))
            {
                foreach (var e in s.exprList)
                {
                    var v = e.Accept(this, env);
                    if(v != Declaration.Type.INT && v != Declaration.Type.BOOL)
                    {
                        throw new TypeCheckerException("Type not supported in print statement", s);
                    }
                    s.types.Add(v);
                }
                return Declaration.Type.VOID;
            }
            StatementTypeChecker se = new StatementTypeChecker();
            FunctionDeclaration tempFunction = env.MainEnv.getFunction(s.id.id);
            if (tempFunction == null || tempFunction.arg.Count != s.exprList.Count)
            {
                throw new TypeCheckerException("Functioncall to '" + s.id.id + "', function does not exist", s);
            }
            int i = 0;
            foreach (var e in tempFunction.arg)
            {
                if (e.type != (s.exprList[i].Accept(this, env)))
                {
                    throw new TypeCheckerException("Type mismatch on argument nr " + i + " on functioncall to function " + s.id.id, s);
                }
                i++;
            }
            return tempFunction.type;
        }
    }

}
