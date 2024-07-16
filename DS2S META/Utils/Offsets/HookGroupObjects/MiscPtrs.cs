using DS2S_META.Utils.DS2Hook;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public class MiscPtrs
    {
        private DS2SHook Hook;
        public PHPointer? AvailableItemBag;
        public PHPointer? SpEffectCtrl;
        public PHPointer? WarpManager;
        

        public MiscPtrs(DS2SHook hook, Dictionary<string, PHPointer> PHPDict)
        {
            Hook = hook;

            AvailableItemBag = HGO.ValOrNull(PHPDict, "AvailableItemBag");
            SpEffectCtrl = HGO.ValOrNull(PHPDict, "SpEffectCtrl");
            WarpManager = HGO.ValOrNull(PHPDict, "WarpManager");
        }

        
    }
}
