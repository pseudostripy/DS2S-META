using DS2S_META.Utils.DS2Hook;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public class FuncListPHP
    {
        private DS2SHook Hook;
        public PHPointer? ItemGive;
        public PHPointer? ItemStruct2dDisplay;
        public PHPointer? GiveSouls;
        public PHPointer? RemoveSouls;
        public PHPointer? SetWarpTargetFunc;
        public PHPointer? WarpFunc;
        public PHPointer? DisplayItemWindow;
        public PHPointer? ApplySpEffect;
        public PHPointer? ItemGiveWindow;
        public PHPointer? DisableSkirtDamage;

        public FuncListPHP(DS2SHook hook, Dictionary<string, PHPointer> PHPDict)
        {
            Hook = hook;

            ItemGive = HGO.ValOrNull(PHPDict, "ItemGiveFuncAoB");
            ItemStruct2dDisplay = HGO.ValOrNull(PHPDict, "ItemStruct2dDisplayAoB");
            GiveSouls = HGO.ValOrNull(PHPDict, "GiveSoulsFuncAoB");
            RemoveSouls = HGO.ValOrNull(PHPDict, "RemoveSoulsFuncAoB");
            SetWarpTargetFunc = HGO.ValOrNull(PHPDict, "SetWarpTargetFuncAoB");
            WarpFunc = HGO.ValOrNull(PHPDict, "WarpFuncAoB");
            DisplayItemWindow = HGO.ValOrNull(PHPDict, "DisplayItemFuncAoB");
            ApplySpEffect = HGO.ValOrNull(PHPDict, "ApplySpEffectAoB");
            ItemGiveWindow = HGO.ValOrNull(PHPDict, "ItemGiveWindowPointer");
            DisableSkirtDamage = HGO.ValOrNull(PHPDict, "DisableSkirtDamageAoB");
        }
    }
}
