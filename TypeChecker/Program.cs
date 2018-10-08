using System;
using System.Text;
using System.IO;
using Parser;
using Evaluator;
using System.Diagnostics;

namespace TypeChecker
{
    class Program
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
                ProgramTypeChecker stc = new ProgramTypeChecker(p);
                Console.WriteLine("pass");
                }
                catch (TypeCheckerException e)
                {
                    Console.WriteLine("{0}", e.Message);
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
