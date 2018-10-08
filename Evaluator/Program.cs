using System;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace Parser
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

                var prg = input.ReadToEnd();
                byte[] data = Encoding.ASCII.GetBytes(prg);

                MemoryStream stream = new MemoryStream(data, 0, data.Length);

                
                Strip stripper = new Strip();
                Scanner l = new Scanner(stream);
                Parser p = new Parser(l);

                if (p.Parse())
                {
                    PrettyBuilder b = new PrettyBuilder();
                    p.program.Pretty(b);




                    if (stripper.strip(prg).Equals(stripper.strip(b.ToString())))
                    {
                        Console.WriteLine("True");
                    }
                    else
                    {
                        Console.WriteLine("False");
                        Console.WriteLine(stripper.strip(b.ToString()) + "\n");
                        Console.WriteLine(stripper.strip(prg));

                    }
                }
                else
                {
                    Console.WriteLine("False");
                }
        }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

}
    }
}