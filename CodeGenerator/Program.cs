using System;
using Parser;
using TypeChecker;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace CodeGenerator
{
    class MainClass
    {

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage; {0} [-t | <filename>]", Process.GetCurrentProcess().ProcessName);
                return;
            }

            try
            {
                StreamReader input;

                if (args[0] == "-t")
                {
                    input = new StreamReader(Console.OpenStandardInput());
                }
                else
                {
                    input = new StreamReader(args[0]);
                }
                var program = input.ReadToEnd();

                byte[] data = Encoding.ASCII.GetBytes(program);
            MemoryStream stream = new MemoryStream(data, 0, data.Length);
            Scanner l = new Scanner(stream);
            Parser.Parser p = new Parser.Parser(l);

                if (!p.Parse())
                {
                    Console.WriteLine("Parse error");
                    Console.WriteLine(program);
                    return;
                }

            try
            {
                var progTC = new ProgramTypeChecker(p);
                var progGen = new ProgramCodeGenerator();
                var t42prog = progGen.Start(p.program);

                Console.Out.WriteLine(t42prog.ToString());
            }
            catch (TypeCheckerException e)
            {
                Console.WriteLine("Type error {0}", e.Message);
                return;
            }
        }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

}
    }
}
