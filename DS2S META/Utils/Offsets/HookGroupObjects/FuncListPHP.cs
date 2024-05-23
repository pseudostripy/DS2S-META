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
        public PHPointer? SetWarpTargetFuncAoB;
        public PHPointer? WarpFuncAoB;
        public PHPointer? DisplayItemWindow;
        public PHPointer? ApplySpEffectAoB;
        public PHPointer? ItemGiveWindow;

        public FuncListPHP(DS2SHook hook, Dictionary<string, PHPointer> PHPDict)
        {
            Hook = hook;

            ItemGive = ValOrNull(PHPDict, "ItemGiveFuncAoB");
            ItemStruct2dDisplay = ValOrNull(PHPDict, "ItemStruct2dDisplayAoB");
            GiveSouls = ValOrNull(PHPDict, "GiveSoulsFuncAoB");
            RemoveSouls = ValOrNull(PHPDict, "RemoveSoulsFuncAoB");
            SetWarpTargetFuncAoB = ValOrNull(PHPDict, "SetWarpTargetFuncAoB");
            WarpFuncAoB = ValOrNull(PHPDict, "WarpFuncAoB");
            DisplayItemWindow = ValOrNull(PHPDict, "DisplayItemFuncAoB");
            ApplySpEffectAoB = ValOrNull(PHPDict, "ApplySpEffectAoB");
            ItemGiveWindow = ValOrNull(PHPDict, "ItemGiveWindowPointer");
        }

        private static PHPointer? ValOrNull(Dictionary<string, PHPointer> PHPDict, string key)
        {
            PHPDict.TryGetValue(key, out var value);
            return value;
        }
    }
}
