using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    internal class DS2VOffsetsV102 : DS2VOffsets
    {
        public DS2VOffsetsV102() : base()
        {
            LoadingState = new int[] { 0x24, 0x19c, 0xa94, 0x14, 0x4, 0x4c, 0x730 };

            if (Func == null || Core == null)
                return;

            // this is correct but doesn't actually work :/
            //Core.NoGrav = new int[5] { 0x18, 0x4c, 0xa0, 0x8, 0xfc }; 
            PlayerType.ChrNetworkPhantomId = 0x38;

            Func.ApplySpEffectAoB = "55 8b ec 8b 45 08 83 ec 10 56 8b f1";
            Func.ItemStruct2dDisplay = "55 8b ec 8b 45 08 8b 4d 14 53 8b 5d 10 56 33 f6";
            Func.GiveSoulsFuncAoB = "55 8b ec 8b 81 e8 00 00 00 8b 55 08 83 ec 08 56";
        

        }
    }
}
