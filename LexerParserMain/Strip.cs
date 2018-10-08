using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    class Strip
    {
        public Strip()
        {

        }

        public string strip(string str)
        {
            string returnstr = "";
            bool skip = false;
            foreach (char x in str)
            {
                if (skip != true)
                {
                    if (x.ToString().Equals(" ") || x.ToString().Equals("\n") || x.ToString().Equals("\t") || x.ToString().Equals("\r") || x.ToString().Equals("\r\n"))
                    {

                    }
                    else
                    {
                        returnstr += x;
                    }
                }
                else
                {
                    skip = false;
                }

            }

            return returnstr;
        }
        


    }
}
