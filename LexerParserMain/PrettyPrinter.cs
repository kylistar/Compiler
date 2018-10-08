using System.Collections.Generic;
using System.Text;

namespace Parser
{
    public class PrettyBuilder
    {
        StringBuilder builder;
        int indent = 0;

        public PrettyBuilder()
        {
            builder = new StringBuilder();
        }

        public void Indent()
        {
            indent += 2;
        }

        public void Unindent()
        {
            indent -= 2;
        }

        public void NewLine()
        {
            builder.Append("\n");
            if (indent > 0) builder.Append(new string(' ', indent));
        }

        public void Append(string s)
        {
            builder.Append(s);
        }

        override public string ToString()
        {
            return builder.ToString();
        }

        public void Intersperse(IEnumerable<IPretty> pretties, string separator)
        {
            var first = true;
            if (pretties == null) return;
            foreach (var p in pretties)
            {
                if (!first) Append(separator);
                p.Pretty(this);
                first = false;
            }
        }
        public void PrettyStm(IEnumerable<IPretty> pretties)
        {
            if (pretties == null) return;
            foreach (var p in pretties)
            {
                p.Pretty(this);
            }
        }

    }

    public interface IPretty
    {
        void Pretty(PrettyBuilder builder);
    }
}
