using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    internal class DS2SOffsetsV102 : DS2SOffsets
    {
        public DS2SOffsetsV102() : base()
        {
            // V1.02
            PlayerStatsOffsets = new int[] { 0x20, 0x28, 0x110, 0x70, 0xA0, 0x170, 0x718 };

            if (Func == null)
                return;

            Func.DisplayItem = "48 8B 89 D8 00 00 00 48 85 C9 0F 85 40 5E 00 00";
            Func.ApplySpEffectAoB = "E9 ? ? ? ? E9 ? ? ? ? 50 5A 41 51 59";
        }
    }
}
