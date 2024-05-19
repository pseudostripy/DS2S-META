using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.CodeLocators
{
    /// <summary>
    /// Provides an AoB + offset which defines a relative jump
    /// to the current instruction. The output pointer is found at
    /// ptrAob + instrlen + reljumpval
    /// </summary>
    internal class RelInstructionAOBCL : AobCodeLocator
    {
        internal int InstrLength; // reljump starts after instruction completes
        internal int[] ChildOffsets; // offsets to follow once you've reached base for this ptr

        public RelInstructionAOBCL(string aob, int reljumpoff, int instrlen, params int[] childOffsets) : base(aob)
        {
            Offset = reljumpoff;
            InstrLength = instrlen;
            ChildOffsets = childOffsets;
        }

        public override PHPointer Init(PHook PH)
        {
            return PH.RegisterRelativeAOB(AoB, Offset, InstrLength, ChildOffsets);
        }

    }
}
