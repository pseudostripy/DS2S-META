using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    internal class DS2VOffsetsV111 : DS2VOffsets
    {
        public DS2VOffsetsV111() : base()
        {
            LoadingState = new int[] { 0x24, 0x14, 0x2fc, 0x54, 0x870 };

            if (Func == null)
                return;
            
            Func.ApplySpEffectAoB = "E9 ? ? ? ? 8B 45 F4 83 C0 01 89 45 F4 E9 ? ? ? ?";
          
        }
    }
}
