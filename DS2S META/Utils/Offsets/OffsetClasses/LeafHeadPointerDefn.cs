using DS2S_META.Utils.Offsets.OffsetClasses;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    public class LeafHeadPointerDefn : PointerDefn
    {
        // Define a pointer by "taking the &address" of a leaf defined by offsets
        public LeafHeadPointerDefn(string id, List<LocatorDefn> locators) : base(id, locators)
        {
        }
        public LeafHeadPointerDefn(string id, params LocatorDefn[] locators) : base(id, locators)
        {
        }

        public override PHPointer Register()
        {
            return null;
        }
    }
}
