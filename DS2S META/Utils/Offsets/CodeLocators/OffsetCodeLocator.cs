using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.CodeLocators
{
    public abstract class OffsetCodeLocator : CodeLocator
    {
        public string ParentPtrId;
        public List<int> Offsets;

        public OffsetCodeLocator(string parentPtrId, List<int> offsets)
        {
            ParentPtrId = parentPtrId;
            Offsets = offsets;
        }
    }
}
