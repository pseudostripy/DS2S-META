using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.CodeLocators
{
    public class ChildPointerCL : OffsetCodeLocator
    {
        public ChildPointerCL(string parentPtrId, List<int> offsets) : base(parentPtrId, offsets)
        {
        }

        public PHPointer InitFromParent(PHook PH, PHPointer parent)
        {
            // Sanity check
            //if (parent.Resolve() == IntPtr.Zero)
            //    throw new Exception("You shouldn't get here. Fail in logic that checks parentPtr is resolved");

            // I think technically the parent doesn't need to resolve yet as long as it exists?

            return PH.CreateChildPointer(parent, Offsets.ToArray());
        }
    }
}
