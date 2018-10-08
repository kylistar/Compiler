using System;
using System.Text;
using System.Collections.Generic;


namespace CodeGenerator
{

    public class T42Program
    {
        public List<T42Instruction> instructions = new List<T42Instruction>(100);
        int line = 0;


        public T42Program()
        {
        }

        public int Line { get => line; }

        public void Emit(T42Instruction inst)
        {
            instructions.Add(inst);
            line++;
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();

            var lineno = 0;
            foreach (var i in instructions)
            {
                b.AppendFormat("{0, -3} {1}\n", lineno++, i.ToString());
            }

            return b.ToString();
        }
    }

    public class T42Instruction
    {
        public enum OPCODE
        {
            PUSHINT, PUSHBOOL, LVAL, RVALINT, RVALBOOL, ASSINT, ASSBOOL,
            ADD, SUB, MULT, DIV, NEG, AND, OR, NOT, EQBOOL, EQINT,
            LTINT, LEINT, DECL, POP, BRF, BRA, WRITEINT, WRITEBOOL,
            LINK, UNLINK, RTS, BSR, END,
            LABEL
        }

        public OPCODE opcode;
        public int argument;
        public string target;

        public T42Instruction(OPCODE opcode)
        {
            this.opcode = opcode;
        }

        public T42Instruction(OPCODE opcode, int argument)
        {
            this.opcode = opcode;
            this.argument = argument;
        }

        public T42Instruction(OPCODE opcode, string target)
        {
            this.opcode = opcode;
            this.target = target;
        }

        override public string ToString()
        {
            switch (opcode)
            {
                case OPCODE.LABEL:
                    return target;

                case OPCODE.BSR:
                    if (argument == 0)
                    {
                        return opcode.ToString() + " " + target;
                    }
                    return opcode.ToString() + " " + argument;

                case OPCODE.PUSHINT:
                    return opcode.ToString() + " " + argument.ToString();
                case OPCODE.PUSHBOOL:
                    return opcode.ToString() + " " + target;
                case OPCODE.POP:
                    return opcode.ToString() + " " + argument.ToString();
                case OPCODE.BRF:
                    return opcode.ToString() + " " + argument.ToString();
                case OPCODE.BRA:
                    return opcode.ToString() + " " + argument.ToString();
                case OPCODE.DECL:
                    return opcode.ToString() + " " + argument.ToString();
                case OPCODE.LVAL:
                    return opcode.ToString() + " " + argument.ToString() + "(FP)";
                case OPCODE.RVALINT:
                    return opcode.ToString() + " " + argument.ToString() + "(FP)";
                case OPCODE.RVALBOOL:
                    return opcode.ToString() + " " + argument.ToString() + "(FP)";
                case OPCODE.SUB:
                    return opcode.ToString();
                case OPCODE.MULT:
                    return opcode.ToString();
                case OPCODE.DIV:
                    return opcode.ToString();
                case OPCODE.NEG:
                    return opcode.ToString();
                case OPCODE.AND:
                    return opcode.ToString();
                case OPCODE.OR:
                    return opcode.ToString();
                case OPCODE.NOT:
                    return opcode.ToString();
                case OPCODE.EQBOOL:
                    return opcode.ToString();
                case OPCODE.LTINT:
                    return opcode.ToString();
                case OPCODE.LEINT:
                    return opcode.ToString();
                case OPCODE.ASSINT:
                    return opcode.ToString();
                case OPCODE.ASSBOOL:
                    return opcode.ToString();
                case OPCODE.ADD:
                    return opcode.ToString();
                case OPCODE.EQINT:
                    return opcode.ToString();
                case OPCODE.WRITEINT:
                    return opcode.ToString(); 
                case OPCODE.WRITEBOOL:
                    return opcode.ToString(); 
                case OPCODE.LINK:
                    return opcode.ToString();
                case OPCODE.UNLINK:
                    return opcode.ToString();
                case OPCODE.RTS:
                    return opcode.ToString();
                case OPCODE.END:
                    return opcode.ToString() + " ";
            }

            throw new NotImplementedException();
        }
    }
}