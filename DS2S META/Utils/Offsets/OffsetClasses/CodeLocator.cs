using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    public class OffsetLocator : CodeLocator
    {
        public string ParentPtrId;
        public List<int> Offsets;

        public OffsetLocator(string parentPtrId, List<int> offsets)
        {
            ParentPtrId = parentPtrId;
            Offsets = offsets;
        }

        public override PHPointer Init(PHook PH) 
        {
            throw new NotImplementedException();
        }
        
    }
}
