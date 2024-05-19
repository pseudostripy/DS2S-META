using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.CodeLocators
{
    /// <summary>
    /// Provides an AoB and gets the ptr of where this is
    /// found (first found instance only)
    /// </summary>
    internal class AbsoluteAOBCL : AobCodeLocator
    {
        public AbsoluteAOBCL(string aob) : base(aob)
        {
            Offset = 0;
        }

        public override PHPointer Init(PHook PH)
        {
            return PH.RegisterAbsoluteAOB(AoB);
        }

    }
}
