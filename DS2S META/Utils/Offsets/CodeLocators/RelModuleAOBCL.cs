using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.CodeLocators
{
    /// <summary>
    /// Defines a pointer by an AoB scan + offset which defines a
    /// memory address relative to the process Module start address.
    /// </summary>
    internal class RelModuleAOBCL : AobCodeLocator
    {
        public RelModuleAOBCL(string aob, int offset) : base(aob)
        {
            Offset = offset;
        }
        
        public override PHPointer Init(PHook PH)
        {
            return PH.RegisterAbsOffsetsAOB(AoB, Offset);
        }
    }
}
