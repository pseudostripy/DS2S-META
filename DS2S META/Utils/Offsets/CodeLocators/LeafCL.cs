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
            // There was an interesting bug here. Basically we don't need this child pointer
            // resolved yet, as long as its resolvable at run-time. For example, the PlayerCtrl
            // pointer isn't set up in the game until you load a game file. Upon setup it always
            // seems to be in the same place as BaseA->0xd0. This means that we *definitely do*
            // need the front end checks to ensure playerCtrl pointers and their children
            // are not called before the pointer is valid.
            var php = PH.CreateChildPointer(parent, Offsets.ToArray());
            return new PHLeaf(php, LeafOffset);
        }
    }
}
