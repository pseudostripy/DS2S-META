using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    internal class AobCodeLocator : CodeLocator
    {
        internal string AoB;
        internal int Offset; // relative scan offset

        public AobCodeLocator(string aob)
        {
            AoB = aob;
            Offset = 0;
        }
        public AobCodeLocator(string aob, int off)
        {
            AoB = aob;
            Offset = off;
        }
    }
}
