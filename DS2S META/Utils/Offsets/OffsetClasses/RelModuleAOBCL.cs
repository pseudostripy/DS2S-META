using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    /// <summary>
    /// Defines a pointer by an AoB scan + offset which defines a
    /// memory address relative to the process Module start address.
    /// </summary>
    internal class RelModuleAOBCL : AobCodeLocator
    {
        internal int AobOffset; // relative read

        public RelModuleAOBCL(string aob)
        {
            AoB = aob;
            AobOffset = 0;
        }
        public RelModuleAOBCL(string aob, int off)
        {
            AoB = aob;
            AobOffset = off;
        }

        public override PHPointer Register()
        {
            throw new NotImplementedException();
        }

    }
}
