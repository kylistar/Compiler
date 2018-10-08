using System;
using System.Collections.Generic;
using Parser;

namespace CodeGenerator
{
    public class Environment
    {
        Stack<Dictionary<string, int>> varOffsets = new Stack<Dictionary<string, int>>();
        public Dictionary<string, int> funcRows = new Dictionary<string, int>();
        public Dictionary<string, List<int>> funcCalls = new Dictionary<string, List<int>>();
        Dictionary<string, FunctionDeclaration> functionDictionary = new Dictionary<string, FunctionDeclaration>();
        int offset = -1, nrArguments;
        public int Offset { get => offset; set => offset = value; }
        public int NrArguments { get => nrArguments; set => nrArguments = value; }

        public Environment()
        {
            varOffsets.Push(new Dictionary<string, int>());
        }
        public int Lookup(string x)
        {
            //int stackIterator = -varOffsets.Peek().Count;
            //bool currentEnv = true;
            foreach (var varEnv in varOffsets)
            {
                if (varEnv.ContainsKey(x))
                {
                    //if (currentEnv == true)
                        return varEnv[x];
      
                    //return varEnv[x] + varEnv.Count + 1 + stackIterator;
                }
                //currentEnv = false;
                //stackIterator += varEnv.Count;
            }
            throw new NotSupportedException();
        }
        public void Declare(string x, int offset)
        {
            var varEnv = varOffsets.Peek();
            varEnv[x] = offset;
        }
        public void EnterScope()
        {
            varOffsets.Push(new Dictionary<string, int>());
        }
        public int ExitScope()
        {
            return varOffsets.Pop().Count;
        }
        public void addRawFunction(string id, Object f)
        {
            functionDictionary[id] = (FunctionDeclaration)f;
        }
        public FunctionDeclaration getRawFunction(string id)
        {
            if (functionDictionary.ContainsKey(id))
            {
                return functionDictionary[id];
            }
            return null;
        }
    }

    public class ProgramCodeGenerator
    {
        T42Program emitter = new T42Program();
        public ProgramCodeGenerator()
        {
        }
        public T42Program Start(Declaration p)
        {
            Environment env = new Environment();
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.DECL, 1));
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.POP, 0));
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.BSR));
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.END));
            env.funcCalls["main"] = new List<int>();
            env.funcCalls["main"].Add(2);
            DeclarationCodeGenerator dg = new DeclarationCodeGenerator(emitter);
            p.Accept(dg, env);
            StatementCodeGenerator sg = new StatementCodeGenerator(emitter);

            foreach (KeyValuePair<string, List<int>> funcCalls in env.funcCalls)
            {
                foreach(var funcCall in funcCalls.Value)
                {
                    emitter.instructions[funcCall].argument = env.funcRows[funcCalls.Key];
                }
            }
            return emitter;
        }
    }

    public class DeclarationCodeGenerator : IDeclarationVisitor<Object, Environment>
    {
        T42Program emitter;
        StatementCodeGenerator sg;

        public DeclarationCodeGenerator(T42Program emitter)
        {
            this.emitter = emitter;
            sg = new StatementCodeGenerator(emitter);
        }

        public Object Visit(SequenceDeclaration s, Environment env)
        {
            s.head.Accept(this, env);
            s.tail.Accept(this, env);
            return null;
        }
        public Object Visit(FunctionDeclaration s, Environment env)
        {
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.LABEL, "[" + s.id.id + "]"));
            int os = 2;
            env.EnterScope();
            foreach (var arg in s.arg)
            {
                env.Declare(arg.id.id, os);
                os++;
            }
            int oldOffset = env.Offset;
            env.Offset = -1;
            env.NrArguments = s.arg.Count;
            env.funcRows[s.id.id] = emitter.Line;
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.LINK));
            s.stm.Accept(sg, env);
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.UNLINK));
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.RTS));
            env.ExitScope();
            env.Offset = oldOffset;

            return null;
        }
        public Object Visit(TypeDeclaration s, Environment env)
        {
            env.Declare(s.id.id, env.Offset--);
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.DECL, 1));
            return null;
        }
    }

    public class StatementCodeGenerator : IStatementVisitor<Object, Environment>
    {
        T42Program emitter;
        ExpressionCodeGenerator eg;

        public StatementCodeGenerator(T42Program emitter)
        {
            this.emitter = emitter;
            eg = new ExpressionCodeGenerator(emitter);
        }

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
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.LVAL, env.NrArguments + 2));
            s.returnValue.Accept(eg, env);
            if (s.type == Declaration.Type.INT)
            {
                emitter.Emit(new T42Instruction(T42Instruction.OPCODE.ASSINT));
            }
            else
            {
                emitter.Emit(new T42Instruction(T42Instruction.OPCODE.ASSBOOL));
            }
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.UNLINK));
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.RTS));
            return null;
        }
        public Object Visit(returnStatement s, Environment env)
        {
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.UNLINK));
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.RTS));
            return null;
        }
        public Object Visit(DeclStatement s, Environment env)
        {
            DeclarationCodeGenerator dg = new DeclarationCodeGenerator(emitter);
            s.decl.Accept(dg, env);
            return null;
        }
        public Object Visit(BlockStatement s, Environment env)
        {

            env.EnterScope();

            foreach (var stm in s.stmList)
            {
                stm.Accept(this, env);
            }
            env.ExitScope();
            return null;
        }
        public Object Visit(ifStatement s, Environment env)
        {
            s.exp.Accept(eg, env);
            int loc = emitter.Line;
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.BRF));

            env.EnterScope();
            //emitter.Emit(new T42Instruction(T42Instruction.OPCODE.LINK));
            s.stm.Accept(this, env);
            int targetAddr = env.ExitScope();
            //emitter.Emit(new T42Instruction(T42Instruction.OPCODE.UNLINK));

            emitter.instructions[loc].argument = emitter.Line;
            return null;
        }
        public Object Visit(whileStatement s, Environment env)
        {
            int locBefore = emitter.Line;
            s.exp.Accept(eg, env);
            int loc = emitter.Line;
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.BRF));

            //emitter.Emit(new T42Instruction(T42Instruction.OPCODE.LINK));
            env.EnterScope();
            s.stm.Accept(this, env);
            int targetAddr = env.ExitScope();
            //emitter.Emit(new T42Instruction(T42Instruction.OPCODE.UNLINK));


            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.BRA, locBefore));

            emitter.instructions[loc].argument = emitter.Line;
            return null;
        }
        public Object Visit(ExprStatement s, Environment env)
        {
            s.exp.Accept(eg, env);
            return null;
        }
        public Object Visit(ifElseStatement s, Environment env)
        {
            s.exp.Accept(eg, env);
            int loc = emitter.Line;
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.BRF));

            env.EnterScope();
            s.ifstm.Accept(this, env);
            int targetAddr = env.ExitScope();


            int loc2 = emitter.Line;
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.BRA));
            emitter.instructions[loc].argument = emitter.Line;

            env.EnterScope();
            s.elsestm.Accept(this, env);
            targetAddr = env.ExitScope();

            targetAddr = emitter.Line;
            emitter.instructions[loc2].argument = emitter.Line;

            return null;
        }
    }

    public class ExpressionCodeGenerator : IExpressionVisitor<Object, Environment>
    {
        T42Program emitter;
        public ExpressionCodeGenerator(T42Program emitter)
        { 
            this.emitter = emitter;
        }

        public Object Visit(IdentifierExpression e, Environment env)
        {
            var offset = env.Lookup(e.id);
            if (e.type == Declaration.Type.INT)
            {
                emitter.Emit(new T42Instruction(T42Instruction.OPCODE.RVALINT, offset));
            }
            else
            {
                emitter.Emit(new T42Instruction(T42Instruction.OPCODE.RVALBOOL, offset));
            }
            return env.Lookup(e.id);
        }
        public Object Visit(NumberExpression e, Environment env)
        {
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.PUSHINT, e.num));
            return null;
        }
        public Object Visit(BinaryOperatorExpression e, Environment env)
        {
            var vl = e.left.Accept(this, env);
            var vr = e.right.Accept(this, env);

            switch (e.op)
            {
                case BinaryOperatorExpression.Operator.ADD:
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.ADD));
                    break;
                case BinaryOperatorExpression.Operator.MINUS:
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.SUB));
                    break;
                case BinaryOperatorExpression.Operator.MULTI:
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.MULT));
                    break;
                case BinaryOperatorExpression.Operator.DIV:
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.DIV));
                    break;
                case BinaryOperatorExpression.Operator.AND:
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.AND));
                    break;
                case BinaryOperatorExpression.Operator.LESS:
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.LTINT));
                    break;
                case BinaryOperatorExpression.Operator.MORE:
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.LEINT));
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.NOT));
                    break;
                case BinaryOperatorExpression.Operator.MOREEQ:
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.LTINT));
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.NOT));
                    break;
                case BinaryOperatorExpression.Operator.LESSEQ:
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.LEINT));
                    break;
                case BinaryOperatorExpression.Operator.EQ:
                    if (e.type == Declaration.Type.INT)
                    {
                        emitter.Emit(new T42Instruction(T42Instruction.OPCODE.EQINT));
                    }
                    else
                    {
                        emitter.Emit(new T42Instruction(T42Instruction.OPCODE.EQBOOL));
                    }
                    break;
                case BinaryOperatorExpression.Operator.NOTEQ:
                    if (e.type == Declaration.Type.INT)
                    {
                        emitter.Emit(new T42Instruction(T42Instruction.OPCODE.EQINT));
                        emitter.Emit(new T42Instruction(T42Instruction.OPCODE.NOT));
                    }
                    else
                    {
                        emitter.Emit(new T42Instruction(T42Instruction.OPCODE.EQBOOL));
                        emitter.Emit(new T42Instruction(T42Instruction.OPCODE.NOT));
                    }
                    break;
                case BinaryOperatorExpression.Operator.OR:
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.OR));
                    break;
            }
            return null;
        }
        public Object Visit(AssignmentExpression s, Environment env)
        {
            IdentifierExpression temp1;
            AssignmentExpression temp2;
            bool isId = true;
            int offset = 0;
            if (s.lo.GetType() == typeof(IdentifierExpression))
            {
                temp1 = (IdentifierExpression)s.lo;
                offset = env.Lookup(temp1.id);
            }
            if (s.lo.GetType() == typeof(AssignmentExpression))
            {
                temp2 = (AssignmentExpression)s.lo;
                temp1 = (IdentifierExpression)temp2.ro;
                offset = env.Lookup(temp1.id);
                isId = false;
            }
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.LVAL, offset));
            s.ro.Accept(this, env);
            if (s.type == Declaration.Type.INT)
            {
                emitter.Emit(new T42Instruction(T42Instruction.OPCODE.ASSINT));
            }
            else
            {
                emitter.Emit(new T42Instruction(T42Instruction.OPCODE.ASSBOOL));
            }
            if(isId == false)
                s.lo.Accept(this, env);

            return null;
        }
        public Object Visit(BoolExpression s, Environment env)
        {
            if(s.binvalue)
                emitter.Emit(new T42Instruction(T42Instruction.OPCODE.PUSHBOOL, "true"));
            else
                emitter.Emit(new T42Instruction(T42Instruction.OPCODE.PUSHBOOL, "false"));
            return null;
        }
        public Object Visit(UnaryOperatorExpression s, Environment env)
        {
            var expr = s.exp.Accept(this, env);

            switch (s.op)
            {
                case UnaryOperatorExpression.Operator.EXCLA:
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.NOT));
                    break;
                case UnaryOperatorExpression.Operator.MINUS:
                    emitter.Emit(new T42Instruction(T42Instruction.OPCODE.NEG));
                    break;
            }              
            return null;
        }
        public Object Visit(FunctionCallExpression s, Environment env)
        {
            if (s.id.id.Equals("print"))
            {
                for (int i = 0; i < s.exprList.Count; i++)
                {
                    s.exprList[i].Accept(this, env);
                    if (s.types[i] == Declaration.Type.INT)
                    {
                        emitter.Emit(new T42Instruction(T42Instruction.OPCODE.WRITEINT));
                    }
                    else
                    {
                        emitter.Emit(new T42Instruction(T42Instruction.OPCODE.WRITEBOOL));
                    }
                }
                return null;
            }

            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.DECL, 1));

            for (int i = s.exprList.Count - 1, j = 0; i >= 0; i--, j++)
            {
                s.exprList[i].Accept(this, env);
            }
            if(env.funcCalls.ContainsKey(s.id.id))
                env.funcCalls[s.id.id].Add(emitter.Line);
            else{
                env.funcCalls[s.id.id] = new List<int>();
                env.funcCalls[s.id.id].Add(emitter.Line);
            }

            //env.NrArguments = s.exprList.Count;
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.BSR));
            emitter.Emit(new T42Instruction(T42Instruction.OPCODE.POP, s.exprList.Count));
            //env.ExitScope();
            return null;
        }
    }
}
