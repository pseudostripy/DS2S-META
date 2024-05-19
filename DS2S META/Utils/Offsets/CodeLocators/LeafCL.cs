using DS2S_META.Utils.Offsets.OffsetClasses;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.CodeLocators
{
    public class LeafCL : OffsetCodeLocator
    {
        public int LeafOffset;
        
        public LeafCL(string parentPtrId, List<int> offsets) : base(parentPtrId, offsets)
        {
            Offsets = offsets.SkipLast(1).ToList(); // overrule base class constructor
            LeafOffset = offsets[^1]; // final offset is to our leaf
        }

        public PHLeaf InitFromParent(PHook PH, PHPointer parent)
        {
            // Sanity check
            if (parent.Resolve() == IntPtr.Zero)
                throw new Exception("You shouldn't get here. Fail in logic that checks parentPtr is resolved");

            var php = PH.CreateChildPointer(parent, Offsets.ToArray());
            return new PHLeaf(php, LeafOffset);
        }
    }
}
