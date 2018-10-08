using System;
using System.Collections.Generic;
using Parser;

namespace Evaluator
{

    public class Environment
    {
        public Dictionary<Type, Declaration.Type> typeDict = new Dictionary<Type, Declaration.Type>
            {
                {typeof(int),Declaration.Type.INT},
                {typeof(bool),Declaration.Type.BOOL}
            };
        Dictionary<string, Object> varEnv = new Dictionary<string, Object>();
        Dictionary<string, FunctionDeclaration> functionDictionary = new Dictionary<string, FunctionDeclaration>();
        Dictionary<string, Declaration.Type> varTypeDict = new Dictionary<string, Declaration.Type>();
        public Declaration.Type typeOfFunc;
        public bool mainBlock = false;
        Environment parentEnvironment = null, mainEnv = null;
        public Environment ParentEnvironment { get => parentEnvironment; set => parentEnvironment = value; }
        public Environment MainEnv { get => mainEnv; set => mainEnv = value; }

        public Object Lookup(string x)
        {
            if (varEnv.ContainsKey(x))
                return varEnv[x];
            else
                return null;
        }
        public void Update(string x, Object v, Environment env)
        {
            if (!env.varEnv.ContainsKey(x) && env.parentEnvironment != null)
                Update(x, v, env.parentEnvironment);

            if (!env.varEnv.ContainsKey(x) && env.parentEnvironment == null)
            {
                Console.WriteLine("INTERPRETATION ERROR: variable does not exist");
                return;
            }

            env.varEnv[x] = v;

        }
        public void Create(TypeDeclaration x, Object v)
        {
            varEnv[x.id.id] = v;
        }
        public void addFunction(string id, Object f)
        {
            functionDictionary[id] = (FunctionDeclaration)f;
        }
        public FunctionDeclaration getFunction(string id)
        {
            if(functionDictionary.ContainsKey(id))
                return mainEnv.functionDictionary[id];

            return null;
        }
    }

    public class ProgramEvaluator
    {
        Environment mainEnvironment = new Environment();
        DeclarationEvaluator sev = new DeclarationEvaluator();
        public ProgramEvaluator(Parser.Parser p)
        {
            p.program.Accept(sev, mainEnvironment);
        }
        public void Start()
        {
            StatementEvaluator se = new StatementEvaluator();
            mainEnvironment.mainBlock = true;
            mainEnvironment.MainEnv = mainEnvironment;
            var func = mainEnvironment.getFunction("main");
            if (func != null)
                func.stm.Accept(se, mainEnvironment);
            else
                Console.WriteLine("No main function found!");
        }

    }

    public class DeclarationEvaluator : IDeclarationVisitor<Object, Environment>
    {
        static StatementEvaluator sv = new StatementEvaluator();
        static ExpressionEvaluator ev = new ExpressionEvaluator();

        public Object Visit(SequenceDeclaration s, Environment env)
        {
            s.head.Accept(this, env);
            s.tail.Accept(this, env);
            return null;
        }
        public Object Visit(FunctionDeclaration s, Environment env)
        {
            env.addFunction(s.id.id, s);
            return null;
        }
        public Object Visit(TypeDeclaration s, Environment env)
        {
            env.Create(s, null);
            return null;
        }
    }

    public class StatementEvaluator : IStatementVisitor<Object, Environment>
    {
        static ExpressionEvaluator ev = new ExpressionEvaluator();
        static DeclarationEvaluator dv = new DeclarationEvaluator();

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
                return s.returnValue.Accept(ev, env);
        }
        public Object Visit(returnStatement s, Environment env)
        {
            return null;
        }
        public Object Visit(DeclStatement s, Environment env)
        {
            s.decl.Accept(dv, env);
            return null;
        }
        public Object Visit(BlockStatement s, Environment env)
        {
            Dictionary<string, Environment> envChoice = new Dictionary<string, Environment>();
            Environment tempEnv;
            if (env.mainBlock == false)
            {
                tempEnv = new Environment
                {
                    ParentEnvironment = env,
                    MainEnv = env.MainEnv
                };
                envChoice["env"] = tempEnv;
            }
            else
            {
                envChoice["env"] = env;
                env.mainBlock = false;
            }
                foreach (var e in s.stmList)
                {
                    var v = e.Accept(this, envChoice["env"]);
                    if (v != null)
                    {
                        return v;
                    }
                }
            return null;
        }
        public Object Visit(ifStatement s, Environment env)
        {
            var v = s.exp.Accept(ev, env);
            if ((bool)v == true)
            {
                return s.stm.Accept(this, env);
            }
            return null;
        }
        public Object Visit(whileStatement s, Environment env)
        {
            while ((bool)s.exp.Accept(ev, env))
            {
                s.stm.Accept(this, env);
            }
            return null;
        }
        public Object Visit(ExprStatement s, Environment env)
        {
            var v = s.exp.Accept(ev, env);
            return null;
        }
        public Object Visit(ifElseStatement s, Environment env)
        {
            if ((bool)s.exp.Accept(ev, env))
            {
                return s.ifstm.Accept(this, env);
            }
            else
            {
                return s.elsestm.Accept(this, env);
            }

        }
    }

    public class ExpressionEvaluator : IExpressionVisitor<Object, Environment>
    {
        public Object Visit(IdentifierExpression e, Environment env)
        {
            var r = env.Lookup(e.id);
            if (r == null && env.ParentEnvironment != null)
                r = Visit(e, env.ParentEnvironment);

            if(r == null && env.ParentEnvironment == null)
                Console.WriteLine("INTERPRETATION ERROR: Variable "+e.id+" not found.");

            return r;
        }
        public Object Visit(NumberExpression e, Environment env)
        {
            return e.num;
        }
        public Object Visit(BinaryOperatorExpression e, Environment env)
        {
            var vl = e.left.Accept(this, env);
            var vr = e.right.Accept(this, env);
            if(vl.GetType() != vr.GetType())
            {
                Console.WriteLine("INTERPRETATION ERROR: type mismatch in binary operator expression");
                return null;
            }
            switch (e.op)
            {
                case BinaryOperatorExpression.Operator.ADD:
                    return (int)vl + (int)vr;
                case BinaryOperatorExpression.Operator.MINUS:
                    return (int)vl - (int)vr;
                case BinaryOperatorExpression.Operator.MULTI:
                    return (int)vl * (int)vr;
                case BinaryOperatorExpression.Operator.DIV:
                    return (int)vl / (int)vr;
                case BinaryOperatorExpression.Operator.AND:
                    return (bool)vl && (bool)vr;
                case BinaryOperatorExpression.Operator.LESS:
                    return (int)vl < (int)vr;
                case BinaryOperatorExpression.Operator.MORE:
                    return (int)vl > (int)vr;
                case BinaryOperatorExpression.Operator.MOREEQ:
                    return (int)vl >= (int)vr;
                case BinaryOperatorExpression.Operator.LESSEQ:
                    return (int)vl <= (int)vr;
                case BinaryOperatorExpression.Operator.EQ:
                    return vl.ToString() == vr.ToString();
                case BinaryOperatorExpression.Operator.NOTEQ:
                    return vl.ToString() != vr.ToString();
                case BinaryOperatorExpression.Operator.OR:
                    return (bool)vl || (bool)vr;
            }
            return null;
        }
        public Object Visit(AssignmentExpression s, Environment env)
        {
            IdentifierExpression temp1;
            AssignmentExpression temp2;
            var v = s.ro.Accept(this, env);
            if(v == null)
            {
                Console.WriteLine("INTERPRETATION ERROR: Right opperand holds no value");
                return null;
            }
            if (s.lo.GetType() == typeof(IdentifierExpression))
            {
                temp1 = (IdentifierExpression)s.lo;
                env.Update(temp1.id, v, env);
            }
            if (s.lo.GetType() == typeof(AssignmentExpression))
            {
                temp2 = (AssignmentExpression)s.lo;
                temp1 = (IdentifierExpression)temp2.ro;
                env.Update(temp1.id, v, env);
                temp2.Accept(this, env);
            }
            return null;
        }
        public Object Visit(BoolExpression s, Environment env)
        {
            return s.binvalue;
        }
        public Object Visit(UnaryOperatorExpression s, Environment env)
        {
            var v = s.exp.Accept(this, env);
            if (s.op == UnaryOperatorExpression.Operator.EXCLA)
            {
                return !(bool)v;
            }
            else
            {
                return -(int)v;
            }
        }
        public Object Visit(FunctionCallExpression s, Environment env)
        {
            if(s.id.id.Equals("print"))
            {
                foreach (var e in s.exprList)
                {
                    var v = e.Accept(this, env);
                    Console.Write(v + " ");
                }
                Console.Write("\n");
                return null;
            }
            StatementEvaluator se = new StatementEvaluator();
            Environment tempEnvironment = new Environment();
            FunctionDeclaration tempFunction = env.MainEnv.getFunction(s.id.id);
            if(tempFunction == null)
            {
                return null;
            }
            tempEnvironment.typeOfFunc = tempFunction.type;
            tempEnvironment.mainBlock = true;
            tempEnvironment.MainEnv = env.MainEnv;
            int i = 0;
            if(tempFunction.arg.Count != s.exprList.Count)
            {
                throw new NotSupportedException("INTERPRETATION ERROR: Not correct number of arguments on functioncall for " + s.id.id);
            }
            foreach(var e in tempFunction.arg)
            {
                tempEnvironment.Create(e, new object());
                tempEnvironment.Update(e.id.id, (s.exprList[i].Accept(this, env)), tempEnvironment);
                i++;
            }
            var t = tempFunction.stm.Accept(se, tempEnvironment);
            if(t == null) { return t; }
            if(tempFunction.type != env.typeDict[t.GetType()])
                throw new NotSupportedException("INTERPRETATION ERROR: Not correct return type for function " + s.id.id);

            return t;
        }
    }
}