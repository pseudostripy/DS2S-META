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
        public PHPointer? ItemGiveFunc;
        public PHPointer? ItemStruct2dDisplay;
        public PHPointer? GiveSouls;
        public PHPointer? RemoveSoulsFuncAoB;
        public PHPointer? SetWarpTargetFuncAoB;
        public PHPointer? WarpFuncAoB;
        public PHPointer? DisplayItemFuncAoB;
        public PHPointer? ApplySpEffectAoB;
        public PHPointer? ItemGiveWindowPointer;

        public FuncListPHP(DS2SHook hook, Dictionary<string, PHPointer> PHPDict)
        {
            Hook = hook;

            ItemGiveFunc = ValOrNull(PHPDict, "ItemGiveFuncAoB");
            ItemStruct2dDisplay = ValOrNull(PHPDict, "ItemStruct2dDisplay");
            GiveSouls = ValOrNull(PHPDict, "GiveSoulsFuncAoB");
            RemoveSoulsFuncAoB = ValOrNull(PHPDict, "RemoveSoulsFuncAoB");
            SetWarpTargetFuncAoB = ValOrNull(PHPDict, "SetWarpTargetFuncAoB");
            WarpFuncAoB = ValOrNull(PHPDict, "WarpFuncAoB");
            DisplayItemFuncAoB = ValOrNull(PHPDict, "DisplayItemFuncAoB");
            ApplySpEffectAoB = ValOrNull(PHPDict, "ApplySpEffectAoB");
            ItemGiveWindowPointer = ValOrNull(PHPDict, "ItemGiveWindowPointer");
        }

        private static PHPointer? ValOrNull(Dictionary<string, PHPointer> PHPDict, string key)
        {
            PHPDict.TryGetValue(key, out var value);
            return value;
        }
    }
}
