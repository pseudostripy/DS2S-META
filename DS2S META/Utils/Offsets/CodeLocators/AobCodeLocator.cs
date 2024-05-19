using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.CodeLocators
{
    internal abstract class AobCodeLocator : CodeLocator
    {
        internal string AoB;
        internal int Offset; // relative AoB offset

        public AobCodeLocator(string aob)
        {
            AoB = aob;
        }

        public abstract PHPointer Init(PHook PH);
    }
}
