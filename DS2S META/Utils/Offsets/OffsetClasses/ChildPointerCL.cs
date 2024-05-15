using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    public class ChildPointerCL : CodeLocator
    {
        public int[] Offsets;

        public ChildPointerCL(string id, params int[] offsets)
        {
            Offsets = offsets;
        }



        public override PHPointer Register()
        {
            return null;
        }
    }
}
