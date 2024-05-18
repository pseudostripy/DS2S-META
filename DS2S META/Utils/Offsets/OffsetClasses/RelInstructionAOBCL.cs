using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    /// <summary>
    /// Provides an AoB + offset which defines a relative jump
    /// to the current instruction. The output pointer is found at
    /// ptrAob + instrlen + reljumpval
    /// </summary>
    internal class RelInstructionAOBCL : CodeLocator
    {
        internal string AoB;
        internal int AoBOffset; // how far into instruction the relative jump int is found
        internal int InstrLength; // reljump starts after instruction completes

        public RelInstructionAOBCL(string aob, int reljumpoff, int instrlen)
        {
            AoB = aob;
            AoBOffset = reljumpoff;
            InstrLength = instrlen;
        }

        public override PHPointer Init(PHook PH)
        {
            throw new NotImplementedException();
        }

    }
}
