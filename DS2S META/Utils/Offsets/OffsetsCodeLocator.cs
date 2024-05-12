using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    internal class OffsetsCodeLocator : CodeLocator
    {
        internal string ParentName;
        internal List<int> Offsets;

        public override PHPointer Register()
        {
            throw new NotImplementedException();
        }

    }
}
