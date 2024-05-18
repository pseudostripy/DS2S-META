using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    /// <summary>
    /// Provides an AoB and gets the ptr of where this is
    /// found (first found instance only)
    /// </summary>
    internal class AbsoluteAOBCL : CodeLocator
    {
        internal string AoB;
        
        public AbsoluteAOBCL(string aob)
        {
            AoB = aob;
        }

        public override PHPointer Init(PHook PH)
        {
            throw new NotImplementedException();
        }

    }
}
