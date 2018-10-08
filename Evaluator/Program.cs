using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Parser;
using System.Diagnostics;

namespace Evaluator
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
                ProgramEvaluator sev = new ProgramEvaluator(p);
                sev.Start();


        }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
    }
