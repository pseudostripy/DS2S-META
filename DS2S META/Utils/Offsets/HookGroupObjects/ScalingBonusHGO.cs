using DS2S_META.Utils.Offsets.OffsetClasses;
using mrousavy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public class ScalingBonusHGO : HGO
    {
        public Dictionary<BNSTYPE, PHLeaf?> BonusScalingDict = new();

        public int GetBonus(BNSTYPE bnstype)
        {
            if (!Hook.Hooked) return 0;
            var phleaf = BonusScalingDict[bnstype];
            return phleaf?.ReadInt32() ?? 0;
        }

        public ScalingBonusHGO(DS2SHook hook, Dictionary<string, PHLeaf?> ptrs) : base(hook)
        {
            foreach (var kvp in ptrs)
            {
                if (Enum.TryParse(kvp.Key, out BNSTYPE e))
                    BonusScalingDict[e] = kvp.Value;
            }
        }

        public override void UpdateProperties()
        {
            // not needed for any ViewModels yet
        }
    }
}
